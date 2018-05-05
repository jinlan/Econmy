using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using HugsLib.Utils;


namespace RimEconmy {

    public class SpecialityWorldManager : WorldComponent {

        public const float ChanceAnimal = 0.001f;
        public const float ChancePlant = 0.002f;
        public const float ChanceResourceRock = 0.0005f;

        private Dictionary<int, Speciality> specialities;

        public SpecialityWorldManager(World world) : base(world) {
        }

        public void generateFresh(string seed) {
            float chanceAnimal;
            float chancePlant;
            float chanceResourceRock;
            chanceAnimal = float.Parse(RimEconmy.SettingData["specialityChanceAnimal"].Value);
            chancePlant = float.Parse(RimEconmy.SettingData["specialityChancePlant"].Value);
            chanceResourceRock = float.Parse(RimEconmy.SettingData["specialityChanceResourceRock"].Value);
            if(specialities == null) {
                specialities = new Dictionary<int, Speciality>((int)(Find.WorldGrid.TilesCount * 4 * 0.001));
            }
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
                        specialities[i] = new Speciality(i, animalKindDef, plantDef, resourceRock);
                    }
                }
            }
        }

        public Speciality getSpeciality(int tile) {
            if(specialities.ContainsKey(tile)) {
                return specialities[tile];
            }
            return null;
        }

        public override void ExposeData() {
            if(Scribe.mode == LoadSaveMode.Saving) {
                int i = 0;
                foreach(KeyValuePair<int, Speciality> kvp in specialities) {
                    int key = kvp.Key;
                    Speciality speciality = kvp.Value;
                    if(speciality != null) {
                        Scribe_Values.Look<int>(ref key, "mbk" + i, 0, true);
                        Scribe_Deep.Look<Speciality>(ref speciality, "mb" + i);
                    }
                }
            }
            if(Scribe.mode == LoadSaveMode.LoadingVars) {
                if(specialities == null) {
                    specialities = new Dictionary<int, Speciality>((int)(Find.WorldGrid.TilesCount * 4 * 0.001));
                }
                for(int i = 0; i <= Find.WorldGrid.TilesCount - 1; i++) {
                    int key = 0;
                    Speciality speciality = null;
                    Scribe_Values.Look<int>(ref key, "mbk" + i, 0);
                    Scribe_Deep.Look<Speciality>(ref speciality, "mb" + i);
                    if(speciality != null) {
                        specialities[key] = speciality;
                    }
                }
            }
        }
    }
}
