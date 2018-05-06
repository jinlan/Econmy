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

        private Dictionary<Settlement, Dictionary<Thing, float>> productionLists;
        private Dictionary<Settlement, List<Speciality>> specialityLists;

        static ProductionWorldManager() {
            singleton = new ProductionWorldManager();
        }

        public ProductionWorldManager() {
            productionLists = new Dictionary<Settlement, Dictionary<Thing, float>>();
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
                return shuffleProductionList(productionLists[settlement]);
            }
            if(getSpecialityList().Count == 0) {
                return new List<Thing>();
            }
            List<ThingDef> materialList = getRawMaterials();
            TechLevel baseTech = settlement.Faction.def.techLevel;
            if(allManufactored == null) {
                allManufactored = from ThingDef thingDef in DefDatabase<ThingDef>.AllDefs
                                  where thingDef.recipeMaker != null && thingDef.tradeability == Tradeability.Stockable
                                  select thingDef;
            }
            Dictionary<Thing, float> productionListWithOrder = new Dictionary<Thing, float>();
            foreach(ThingDef production in allManufactored) {
                if(production.techLevel <= baseTech) {
                    int matchingIngredient = 0;
                    int ingredientTypeCount = 0;
                    ThingDef productionStuff = null;
                    if(production.costList != null) {
                        foreach(ThingCountClass cost in production.costList) {
                            if(materialList.Contains(cost.thingDef)) {
                                matchingIngredient++;
                            }
                            ingredientTypeCount++;
                        }
                    }
                    if(production.MadeFromStuff) {
                        if(materialList.Any((ThingDef material) => {
                            if(material.stuffProps != null && material.stuffProps.CanMake(production)) {
                                productionStuff = material;
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
                        }
                        productionListWithOrder[ThingMaker.MakeThing(production, productionStuff)] = matchingIngredient / ingredientTypeCount;
                    }
                }
            }
            productionLists[settlement] = productionListWithOrder;
            return shuffleProductionList(productionLists[settlement]);
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
                    float moveCost = 2500;
                    Tile tile = grid.tiles[neighborPlace];
                    if(tile == null && tile.WaterCovered) {
                        return 99999;
                    }
                    Season season = GenDate.Season(Find.TickManager.TicksGame, grid.LongLatOf(neighborPlace));
                    switch(season) {
                    case Season.Spring:
                        moveCost += tile.biome.pathCost_spring;
                        break;
                    case Season.Summer:
                    case Season.PermanentSummer:
                        moveCost += tile.biome.pathCost_summer;
                        break;
                    case Season.Fall:
                        moveCost += tile.biome.pathCost_fall;
                        break;
                    case Season.Winter:
                    case Season.PermanentWinter:
                        moveCost += tile.biome.pathCost_winter;
                        break;
                    }
                    moveCost *= grid.GetRoadMovementMultiplierFast(currentPlace, neighborPlace);
                    return (int)moveCost;
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
        private List<Thing> shuffleProductionList(Dictionary<Thing, float> listWithWeight) {
            return listWithWeight.OrderByDescending((KeyValuePair<Thing, float> kvp) => kvp.Value, new PublicExtension.CompareFloat()).ThenBy((KeyValuePair<Thing, float> kvp) => kvp.Key, new PublicExtension.randomOrder<Thing>()).Select((KeyValuePair<Thing, float> arg) => arg.Key).ToList();
        }
    }
}
