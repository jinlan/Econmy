using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using HugsLib.Utils;

namespace RimEconmy {

    public class ProductionWorldManager {

        public static ProductionWorldManager singleton;
        private static IEnumerable<ThingDef> allManufactored;

        private Dictionary<Settlement, List<Thing>> productionLists;
        private Dictionary<Settlement, List<Speciality>> specialityLists;

        static ProductionWorldManager() {
            singleton = new ProductionWorldManager();
        }

        public ProductionWorldManager() {
            productionLists = new Dictionary<Settlement, List<Thing>>();
            specialityLists = new Dictionary<Settlement, List<Speciality>>();
        }
        public List<ThingDef> getRawMaterials() {
            Settlement settlement = TradeSession.trader as Settlement;
            if(settlement == null) {
                return new List<ThingDef>();
            }
            List<Speciality> specialityList = getSpecialityList();
            List<ThingDef> allSpecialityProduction = new List<ThingDef>();
            if(specialityList.Count == 0) {
                return allSpecialityProduction;
            }
            Faction baseFaction = settlement.Faction;
            foreach(Speciality speciality in specialityList) {
                if(speciality.AnimalSpeciality != null) {
                    allSpecialityProduction.Add(speciality.AnimalSpeciality.race);
                }
                allSpecialityProduction.AddRange(speciality.getAllProductions());
            }
            allSpecialityProduction.Remove(ThingDefOf.Silver);
            return allSpecialityProduction;
        }
        public List<Thing> getProductionList() {
            Settlement settlement = TradeSession.trader as Settlement;
            if(settlement == null) {
                return new List<Thing>();
            }
            if(productionLists.ContainsKey(settlement)) {
                return productionLists[settlement];
            }
            List<Thing> list = new List<Thing>();
            if(getSpecialityList().Count == 0) {
                return list;
            }
            List<ThingDef> materialList = getRawMaterials();
            TechLevel baseTech = settlement.Faction.def.techLevel;
            if(allManufactored == null) {
                allManufactored = from ThingDef thingDef in DefDatabase<ThingDef>.AllDefs
                                  where thingDef.recipeMaker != null && thingDef.tradeability == Tradeability.Stockable
                                  select thingDef;
            }
            Dictionary<Thing, float> productionListWithOrder = new Dictionary<Thing, float>();
            ModLogger logger = new ModLogger("production generating: " + settlement.TraderName);
            foreach(ThingDef production in allManufactored) {
                if(production.techLevel <= baseTech) {
                    logger.Message("start check: " + production.label);
                    int matchingIngredient = 0;
                    int ingredientTypeCount = 0;
                    ThingDef productionStuff = null;
                    if(production.costList != null) {
                        foreach(ThingCountClass cost in production.costList) {
                            if(materialList.Contains(cost.thingDef)) {
                                matchingIngredient++;
                                logger.Message("costList check success: " + cost.thingDef.label);
                            }
                            ingredientTypeCount++;
                        }
                    }
                    if(production.MadeFromStuff) {
                        logger.Message("start check stuff");
                        if(materialList.Any((ThingDef material) => {
                            if(material.stuffProps != null && material.stuffProps.CanMake(production)) {
                                productionStuff = material;
                                logger.Message("stuff check success: " + production.label);
                                return true;
                            }
                            return false;
                        })) {
                            matchingIngredient++;
                        }
                        ingredientTypeCount++;
                    }
                    if(matchingIngredient > 0) {
                        if(production.MadeFromStuff && productionStuff == null) {
                            productionStuff = GenStuff.RandomStuffFor(production);
                            logger.Message("adding: " + production.label + " stuff: " + productionStuff.label);
                        } else {
                            logger.Message("adding: " + production.label);
                        }
                        productionListWithOrder[ThingMaker.MakeThing(production, productionStuff)] = matchingIngredient / ingredientTypeCount;
                    }
                }
            }
            list = productionListWithOrder.OrderByDescending((KeyValuePair<Thing, float> kvp) => {
                return kvp.Value;
            }, new PublicExtension.CompareFloat()).Select((KeyValuePair<Thing, float> arg) => arg.Key).ToList();
            productionLists[settlement] = list;
            logger.Message("dumping list");
            logger.Trace(list);
            return list;
        }
        public List<Speciality> getSpecialityList() {
            Settlement settlement = TradeSession.trader as Settlement;
            if(settlement == null) {
                return new List<Speciality>();
            }
            List<Speciality> specialityList;
            if(specialityLists.ContainsKey(settlement)) {
                specialityList = specialityLists[settlement];
            } else {
                specialityList = new List<Speciality>();
                int ticksPerDay = 60000 / 24 * 14;
                WorldGrid grid = Find.WorldGrid;
                WorldPathFinder finder = Find.WorldPathFinder;
                SpecialityWorldManager specialityWorldManager = Find.World.GetComponent<SpecialityWorldManager>();
                finder.FloodPathsWithCost(new List<int> { settlement.Tile }, (int currentPlace, int neighborPlace) => {
                    Tile tile = grid.tiles[neighborPlace];
                    if(tile == null) {
                        return 99999;
                    }
                    Season season = GenDate.Season(Find.TickManager.TicksGame, grid.LongLatOf(neighborPlace));
                    switch(season) {
                    case Season.Spring:
                        return tile.biome.pathCost_spring + 2500;
                    case Season.Summer:
                    case Season.PermanentSummer:
                        return tile.biome.pathCost_summer + 2500;
                    case Season.Fall:
                        return tile.biome.pathCost_fall + 2500;
                    case Season.Winter:
                    case Season.PermanentWinter:
                        return tile.biome.pathCost_winter + 2500;
                    }
                    Log.Error("can't get a season");
                    return 99999;
                }, null, (int currentPlace, float cost) => {
                    if(cost <= ticksPerDay) {
                        Speciality speciality = specialityWorldManager.getSpeciality(currentPlace);
                        if(speciality != null) {
                            specialityList.Add(speciality);
                        }
                        return false;
                    } else {
                        return true;
                    }
                });
                specialityLists[settlement] = specialityList;
            }
            return specialityList;
        }
    }
}
