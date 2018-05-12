using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using HugsLib.Utils;


namespace RimEconomy {

    public class RimEconomyWorldManager : WorldComponent {

        private Dictionary<int, Speciality> tileSpeciality;
        private Dictionary<int, ExposableList<Speciality>> tileNeighboringSpecialities;

        private static IEnumerable<ThingDef> allManufactored;

        private Dictionary<Settlement, Dictionary<Thing, float>> settlementProductionListMap;

        public RimEconomyWorldManager(World world) : base(world) {
            tileSpeciality = new Dictionary<int, Speciality>((int)(Find.WorldGrid.TilesCount * 4 * 0.001));
            tileNeighboringSpecialities = new Dictionary<int, ExposableList<Speciality>>();
            settlementProductionListMap = new Dictionary<Settlement, Dictionary<Thing, float>>();
        }

        public void generateSpecialitiesFresh(string seed) {
            float chanceAnimal;
            float chancePlant;
            float chanceResourceRock;
            chanceAnimal = float.Parse(RimEconomy.SettingData["specialityChanceAnimal"].Value);
            chancePlant = float.Parse(RimEconomy.SettingData["specialityChancePlant"].Value);
            chanceResourceRock = float.Parse(RimEconomy.SettingData["specialityChanceResourceRock"].Value);
            Rand.Seed = GenText.StableStringHash(seed);
            List<Tile> tiles = Find.WorldGrid.tiles;
            Dictionary<BiomeDef, IEnumerable<ThingDef>> biomePlantCache = new Dictionary<BiomeDef, IEnumerable<ThingDef>>();
            IEnumerable<ThingDef> resourceRocks = from d in DefDatabase<ThingDef>.AllDefs
                                                  where d.category == ThingCategory.Building && d.building != null && d.building.isResourceRock
                                                  select d;
            for(int i = 0; i <= Find.WorldGrid.TilesCount - 1; i++) {
                Tile tile = tiles[i];
                if(!tile.WaterCovered) {
                    BiomeDef biome = tile.biome;
                    PawnKindDef animalKindDef = null;
                    ThingDef plantDef = null;
                    ThingDef resourceRock = null;
                    if(Rand.Chance(chanceAnimal * biome.animalDensity)) {
                        animalKindDef = biome.AllWildAnimals.RandomElementByWeight((PawnKindDef def) => biome.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average);
                    }
                    if(Rand.Chance(chancePlant * biome.plantDensity)) {
                        IEnumerable<ThingDef> plants = null;
                        if(biomePlantCache.ContainsKey(biome)) {
                            plants = biomePlantCache[biome];
                        } else {
                            plants = from ThingDef def in biome.AllWildPlants
                                     where def.plant != null && (def.plant.harvestedThingDef != null || (def.plant.sowTags != null && def.plant.sowTags.Contains("Ground")))
                                     select def;
                            biomePlantCache[biome] = plants;
                        }
                        plantDef = plants.RandomElementByWeight((ThingDef def) => biome.CommonalityOfPlant(def));
                    }
                    if(Rand.Chance(chanceResourceRock)) {
                        resourceRock = resourceRocks.RandomElementByWeight((ThingDef def) => def.building.mineableScatterCommonality);
                    }
                    if(animalKindDef != null || plantDef != null || resourceRock != null) {
                        tileSpeciality[i] = new Speciality(i, animalKindDef, plantDef, resourceRock);
                    }
                }
            }
        }

        public Speciality getTileSpeciality(int tile) {
            if(tileSpeciality.ContainsKey(tile)) {
                return tileSpeciality[tile];
            }
            return null;
        }

        public override void ExposeData() {
            Scribe_Collections.Look<int, Speciality>(ref tileSpeciality, "sps", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<int, ExposableList<Speciality>>(ref tileNeighboringSpecialities, "tns", LookMode.Value, LookMode.Deep);
        }

        public List<ThingDef> getSettlementRawMaterials(Settlement settlement) {
            if(settlement == null) {
                return new List<ThingDef>();
            }
            List<Speciality> specialityList = getSettlementSpecialities(settlement);
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
        public List<Thing> getSettlementProductionList(Settlement settlement) {
            if(settlement == null) {
                return new List<Thing>();
            }
            if(settlementProductionListMap.ContainsKey(settlement)) {
                return shuffleProductionList(settlementProductionListMap[settlement]);
            }
            if(getSettlementSpecialities(settlement).Count == 0) {
                return new List<Thing>();
            }
            List<ThingDef> materialList = getSettlementRawMaterials(settlement);
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
            settlementProductionListMap[settlement] = productionListWithOrder;
            return shuffleProductionList(settlementProductionListMap[settlement]);
        }
        public List<Speciality> getTileNeighboringSpecialities(int tile, int distance, float moveCost) {
            ExposableList<Speciality> specialityList;
            if(tileNeighboringSpecialities.ContainsKey(tile)) {
                specialityList = tileNeighboringSpecialities[tile];
            } else {
                specialityList = new ExposableList<Speciality>();
                specialityList.Exposer = (List<Speciality> list) => {
                    Scribe_Collections.Look<Speciality>(ref list, "data", LookMode.Reference);
                };
                WorldGrid grid = Find.WorldGrid;
                WorldPathFinder finder = Find.WorldPathFinder;
                finder.FloodPathsWithCost(new List<int> { tile }, (int currentPlace, int neighborPlace) => {
                    Tile neighborTile = grid.tiles[neighborPlace];
                    if(neighborTile == null && neighborTile.WaterCovered) {
                        return 99999;
                    }
                    Season season = GenDate.Season(Find.TickManager.TicksGame, grid.LongLatOf(neighborPlace));
                    switch(season) {
                    case Season.Spring:
                        moveCost += neighborTile.biome.pathCost_spring;
                        break;
                    case Season.Summer:
                    case Season.PermanentSummer:
                        moveCost += neighborTile.biome.pathCost_summer;
                        break;
                    case Season.Fall:
                        moveCost += neighborTile.biome.pathCost_fall;
                        break;
                    case Season.Winter:
                    case Season.PermanentWinter:
                        moveCost += neighborTile.biome.pathCost_winter;
                        break;
                    }
                    moveCost *= grid.GetRoadMovementMultiplierFast(currentPlace, neighborPlace);
                    return (int)moveCost;
                }, null, (int currentPlace, float cost) => {
                    if(cost <= distance) {
                        Speciality speciality = getTileSpeciality(currentPlace);
                        if(speciality != null) {
                            specialityList.Add(speciality);
                        }
                        return false;
                    } else {
                        return true;
                    }
                });
                tileNeighboringSpecialities[tile] = specialityList;
            }
            return specialityList;
        }
        public List<Speciality> getSettlementSpecialities(Settlement settlement) {
            int ticksPerDay = 60000 / 24 * 14;
            float baseHumanMoveCost = 2500;
            return getTileNeighboringSpecialities(settlement.Tile, ticksPerDay, baseHumanMoveCost);
        }
        private List<Thing> shuffleProductionList(Dictionary<Thing, float> listWithWeight) {
            return listWithWeight.OrderByDescending((KeyValuePair<Thing, float> kvp) => kvp.Value, new PublicExtension.CompareFloat()).ThenBy((KeyValuePair<Thing, float> kvp) => kvp.Key, new PublicExtension.randomOrder<Thing>()).Select((KeyValuePair<Thing, float> arg) => arg.Key).ToList();
        }

    }
}
