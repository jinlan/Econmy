using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using HugsLib.Utils;


namespace RimEconomy {

    public class RimEconomyWorldManager : WorldComponent {

        private Dictionary<int, Speciality> tileSpeciality;
        private Dictionary<int, ExposableList<Speciality>> settlementSpecialities;

        private static IEnumerable<ThingDef> allManufactored;

        private Dictionary<Settlement, Dictionary<Thing, float>> settlementProductionListMap;

        private bool generated = false;

        public RimEconomyWorldManager(World world) : base(world) {
            tileSpeciality = new Dictionary<int, Speciality>((int)(Find.WorldGrid.TilesCount * 4 * 0.001));
            settlementSpecialities = new Dictionary<int, ExposableList<Speciality>>();
            settlementProductionListMap = new Dictionary<Settlement, Dictionary<Thing, float>>();
        }

        private void generateSpecialities(string seed) {
            float chanceAnimal = RimEconomy.SettingFloat["specialityChanceAnimal"].Value;
            float chancePlant = RimEconomy.SettingFloat["specialityChancePlant"].Value;
            float chanceResourceRock = RimEconomy.SettingFloat["specialityChanceResourceRock"].Value;
            bool dontFilterSpeciality = RimEconomy.SettingBool["dontFilterSpeciality"].Value;
            float maxCommonalityOfAnimal = RimEconomy.SettingFloat["maxCommonalityOfAnimal"].Value;
            float maxCommonalityOfPlant = RimEconomy.SettingFloat["maxCommonalityOfPlant"].Value;
            if(seed != null) {
                Rand.Seed = GenText.StableStringHash(seed);
            }
            List<Tile> tiles = Find.WorldGrid.tiles;
            Dictionary<BiomeDef, IEnumerable<ThingDef>> biomePlantCache = new Dictionary<BiomeDef, IEnumerable<ThingDef>>();
            Dictionary<BiomeDef, IEnumerable<PawnKindDef>> biomeAnimalCache = new Dictionary<BiomeDef, IEnumerable<PawnKindDef>>();
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
                        IEnumerable<PawnKindDef> animals = null;
                        if(biomeAnimalCache.ContainsKey(biome)) {
                            animals = biomeAnimalCache[biome];
                        } else {
                            animals = biome.AllWildAnimals;
                            biomeAnimalCache[biome] = animals;
                            Log.Message("-----------deubg for animal----------------");
                            Log.Message("biome: " + biome);
                            foreach(PawnKindDef def in animals) {
                                Log.Message("def: " + def.race + " commonality: " + biome.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average);
                            }
                        }
                        animalKindDef = animals.RandomElementByWeight((PawnKindDef def) => {
                            if(def.race == DefDatabase<ThingDef>.GetNamed("Rat", true)) {
                                return 0;
                            }
                            if(def.wildSpawn_GroupSizeRange.Average > 0) {
                                float commonality = biome.CommonalityOfAnimal(def) / def.wildSpawn_GroupSizeRange.Average;
                                if(!dontFilterSpeciality && commonality >= maxCommonalityOfAnimal) {
                                    return 0;
                                }
                                return commonality;
                            }
                            return 0;
                        });
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
                            Log.Message("-----------deubg for plant----------------");
                            Log.Message("biome: " + biome);
                            foreach(ThingDef def in plants) {
                                Log.Message("def: " + def + " commonality: " + biome.CommonalityOfPlant(def));
                            }
                        }
                        plantDef = plants.RandomElementByWeight((ThingDef def) => {
                            if(def == DefDatabase<ThingDef>.GetNamed("PlantGrass", true)) {
                                return 0;
                            }
                            float commonality = biome.CommonalityOfPlant(def);
                            if(!dontFilterSpeciality && commonality >= maxCommonalityOfPlant) {
                                return 0;
                            }
                            return commonality;
                        });
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

        public void generateSpecialitiesFresh(string seed = null) {
            generateSpecialities(seed);
            generated = true;
        }

        public void GenerateSpecialitiesFromScribe(string seed = null) {
            if(!generated) {
                generateSpecialities(seed);
            }
        }

        public Speciality getTileSpeciality(int tile) {
            if(tileSpeciality.ContainsKey(tile)) {
                return tileSpeciality[tile];
            }
            return null;
        }

        public override void ExposeData() {
            Scribe_Values.Look<bool>(ref generated, "g", false, true);
            Scribe_Collections.Look<int, Speciality>(ref tileSpeciality, "sps", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<int, ExposableList<Speciality>>(ref settlementSpecialities, "tns", LookMode.Value, LookMode.Deep);
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
        protected ExposableList<Speciality> getTileNeighboringSpecialities(int tile, int timeLimit, float moveCost, Func<int, int, Speciality, bool> extraValidator = null) {
            ExposableList<Speciality> specialityList = new ExposableList<Speciality>();
            specialityList.Exposer = (List<Speciality> list) => {
                Scribe_Collections.Look<Speciality>(ref list, "data", LookMode.Reference);
            };
            WorldGrid grid = Find.WorldGrid;
            Dictionary<int, float> timeCostToTile = new Dictionary<int, float>();
            Find.WorldFloodFiller.FloodFill(tile, (int currentPlace) => {
                if(Find.World.Impassable(currentPlace)) {
                    return false;
                }
                Tile tileObject = grid.tiles[currentPlace];
                if(tileObject == null || tileObject.WaterCovered || tileObject.hilliness == Hilliness.Impassable) {
                    return false;
                }
                if(currentPlace == tile) {
                    timeCostToTile[currentPlace] = 0;
                    return true;
                }
                List<int> neighbors = new List<int>(6);
                grid.GetTileNeighbors(currentPlace, neighbors);
                float bestNeighborTimeCost = -1;
                int bestNeighbor = currentPlace;
                foreach(int neighborTile in neighbors) {
                    if(timeCostToTile.ContainsKey(neighborTile)) {
                        if(bestNeighborTimeCost < 0) {
                            bestNeighborTimeCost = timeCostToTile[neighborTile];
                        }
                        if(timeCostToTile[neighborTile] < bestNeighborTimeCost) {
                            bestNeighborTimeCost = timeCostToTile[neighborTile];
                            bestNeighbor = neighborTile;
                        }
                    }
                }
                float timeCostToCurrentPlace = bestNeighborTimeCost + getMoveCost(bestNeighbor, currentPlace, moveCost);
                if(timeCostToCurrentPlace <= timeLimit) {
                    timeCostToTile[currentPlace] = timeCostToCurrentPlace;
                    return true;
                } else {
                    return false;
                }
            }, (int currentPlace, int distanceInTiles) => {
                Speciality speciality = getTileSpeciality(currentPlace);
                if(speciality != null) {
                    specialityList.Add(speciality);
                }
                if(extraValidator != null) {
                    return extraValidator(currentPlace, distanceInTiles, speciality);
                }
                return false;
            });
            return specialityList;
        }
        public List<Speciality> getSettlementSpecialities(Settlement settlement) {
            return getSettlementTileSpecialities(settlement.Tile);
        }
        private float getMoveCost(int from, int to, float moveCost) {
            return ((moveCost + getSeasonMoveCost(from)) / 2 + (moveCost + getSeasonMoveCost(to)) / 2) * Find.WorldGrid.GetRoadMovementMultiplierFast(from, to);
        }
        private float getSeasonMoveCost(int currentPlace) {
            WorldGrid grid = Find.WorldGrid;
            Tile currentTile = grid.tiles[currentPlace];
            Season season = GenDate.Season(Find.TickManager.TicksGame, grid.LongLatOf(currentPlace));
            switch(season) {
            case Season.Spring:
                return currentTile.biome.pathCost_spring;
            case Season.Summer:
            case Season.PermanentSummer:
                return currentTile.biome.pathCost_summer;
            case Season.Fall:
                return currentTile.biome.pathCost_fall;
            case Season.Winter:
            case Season.PermanentWinter:
                return currentTile.biome.pathCost_winter;
            }
            Log.Error("unable to know seasonal move cost");
            return 0;
        }
        public List<Speciality> getSettlementTileSpecialities(int tile) {
            ExposableList<Speciality> specialityList = null;
            if(settlementSpecialities.ContainsKey(tile)) {
                specialityList = settlementSpecialities[tile];
            } else {
                float baseHumanMoveCost = 2500;
                int ticksPerDay = 60000 / 24 * 16;
                int searchDistance = ticksPerDay * 2;
                float moveCost = baseHumanMoveCost;
                Func<int, int, Speciality, bool> extraValidator = null;
                bool hasMountable = false;
                if(RimEconomy.GiddyUpCoreType != null) {
                    extraValidator = (int currentPlace, int distanceInTiles, Speciality speciality) => {
                        if(speciality != null && speciality.AnimalSpeciality != null) {
                            bool mountable = (bool)RimEconomy.GiddyUpCoreType.GetMethod("isAllowedInModOptions", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { speciality.AnimalSpeciality.race.defName });
                            if(mountable) {
                                hasMountable = true;
                                return true;
                            }
                        }
                        return false;
                    };
                }
                specialityList = getTileNeighboringSpecialities(tile, searchDistance, moveCost, extraValidator);
                if(RimEconomy.GiddyUpCoreType != null && hasMountable) {
                    moveCost = moveCost / (1 + RimEconomy.GiddyUpCaravanBonus / 100);
                    specialityList = getTileNeighboringSpecialities(tile, searchDistance, moveCost);
                }
                settlementSpecialities[tile] = specialityList;
            }
            return specialityList;
        }
        private List<Thing> shuffleProductionList(Dictionary<Thing, float> listWithWeight) {
            return listWithWeight.OrderByDescending((KeyValuePair<Thing, float> kvp) => kvp.Value, new PublicExtension.CompareFloat()).ThenBy((KeyValuePair<Thing, float> kvp) => kvp.Key, new PublicExtension.randomOrder<Thing>()).Select((KeyValuePair<Thing, float> arg) => arg.Key).ToList();
        }

    }
}
