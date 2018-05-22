using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using HugsLib.Settings;

namespace RimEconomy {

    public class SpecialityMapManager : MapComponent {

        public const float PawnExtraSpawn = 0.5f;
        public const float PlantExtraSpawn = 0.15f;
        public const float ResourceRockExtraSpawn = 1f;

        private float pawnExtraSpawn;
        private float plantExtraSpawn;
        private float resourceRockExtraSpawn;

        private Speciality speciality;

        public SpecialityMapManager(Map map) : base(map) {
        }

        public void Generate(Speciality speciality) {
            this.speciality = speciality;
            readSetting();
            generateAnimals();
            map.regionAndRoomUpdater.Enabled = false;
            generatePlants();
            generateResourceRocks();
            map.regionAndRoomUpdater.Enabled = true;
        }

        public override void MapComponentTick() {
            if(speciality == null) {
                return;
            }
            IntVec3 place;
            if(speciality.AnimalSpeciality != null && Find.TickManager.TicksGame % 1210 == 0 && Rand.Value < 0.0268888883f * map.wildSpawner.DesiredAnimalDensity(map) * (1 + pawnExtraSpawn) && RCellFinder.TryFindRandomPawnEntryCell(out place, this.map, CellFinder.EdgeRoadChance_Animal, null)) {
                spawnAnimalsAt(place);
            }
            ThingDef plantDef = speciality.PlantSpeciality;
            if(plantDef != null) {
                float num = map.gameConditionManager.AggregatePlantDensityFactor();
                if(num > 0.0001f) {
                    int num2 = map.Size.x * 2 + map.Size.z * 2;
                    float num3 = 650f / ((float)num2 / 100f);
                    int num4 = (int)(num3 / num);
                    if(num4 <= 0 || Find.TickManager.TicksGame % num4 == 0) {
                        if(RCellFinder.TryFindRandomCellToPlantInFromOffMap(plantDef, map, out place)) {
                            GenPlantReproduction.TryReproduceFrom(place, plantDef, SeedTargFindMode.MapEdge, map);
                        }
                    }
                }
            }
        }

        public override void ExposeData() {
            Scribe_References.Look<Speciality>(ref speciality, "sp");
            if(Scribe.mode == LoadSaveMode.LoadingVars) {
                readSetting();
            }
        }

        private void readSetting() {
            SettingHandle<string>.ValueChanged readAnimalSetting = (string value) => {
                pawnExtraSpawn = float.Parse(value);
            };
            SettingHandle<string>.ValueChanged readPlantSetting = (string value) => {
                plantExtraSpawn = float.Parse(value);
            };
            SettingHandle<string>.ValueChanged readResourceRockSetting = (string value) => {
                resourceRockExtraSpawn = float.Parse(value);
            };
            readAnimalSetting(RimEconomy.SettingData["pawnExtraSpawn"].Value);
            readPlantSetting(RimEconomy.SettingData["plantExtraSpawn"].Value);
            readResourceRockSetting(RimEconomy.SettingData["resourceRockExtraSpawn"].Value);
            RimEconomy.SettingData["pawnExtraSpawn"].OnValueChanged = readAnimalSetting;
            RimEconomy.SettingData["plantExtraSpawn"].OnValueChanged = readPlantSetting;
            RimEconomy.SettingData["resourceRockExtraSpawn"].OnValueChanged = readResourceRockSetting;
        }
        private void spawnAnimalAt(IntVec3 place, int randomInRange, PawnKindDef animalKingDef) {
            int radius = Mathf.CeilToInt(Mathf.Sqrt((float)animalKingDef.wildSpawn_GroupSizeRange.max));
            for(int i = 0; i <= randomInRange; i++) {
                IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(place, map, radius, null);
                Pawn newAnimal = PawnGenerator.GeneratePawn(animalKingDef, null);
                GenSpawn.Spawn(newAnimal, loc2, map);
            }
        }
        private void spawnAnimalsAt(IntVec3 place) {
            PawnKindDef animalKingDef = speciality.AnimalSpeciality;
            if(animalKingDef == null) {
                return;
            }
            if(!map.mapTemperature.SeasonAcceptableFor(animalKingDef.race)) {
                return;
            }
            if(pawnExtraSpawn <= 0) {
                return;
            }
            float desiredTotalWeight = map.wildSpawner.DesiredTotalAnimalWeight(map) * (1 + pawnExtraSpawn);
            float currentWeight = map.wildSpawner.CurrentTotalAnimalWeight(map);
            if(currentWeight <= desiredTotalWeight) {
                spawnAnimalAt(place, animalKingDef.wildSpawn_GroupSizeRange.RandomInRange, animalKingDef);
            }
        }
        private void generateAnimals() {
            PawnKindDef animalKingDef = speciality.AnimalSpeciality;
            if(animalKingDef == null) {
                return;
            }
            if(!map.mapTemperature.SeasonAcceptableFor(animalKingDef.race)) {
                return;
            }
            if(pawnExtraSpawn <= 0) {
                return;
            }
            float desiredTotalWeight = map.wildSpawner.DesiredTotalAnimalWeight(map) * (1 + pawnExtraSpawn);
            float currentWeight = map.wildSpawner.CurrentTotalAnimalWeight(map);
            int randomInRange = animalKingDef.wildSpawn_GroupSizeRange.RandomInRange;
            while(currentWeight <= desiredTotalWeight) {
                IntVec3 place = RCellFinder.RandomAnimalSpawnCell_MapGen(map);
                spawnAnimalAt(place, randomInRange, animalKingDef);
                currentWeight += (randomInRange + 1) * animalKingDef.wildSpawn_EcoSystemWeight;
            }
        }
        private void spawnPlant(IntVec3 place, MapGenFloatGrid caves, float desiredTotalDensity, ThingDef plantDef) {
            if(place.GetEdifice(map) == null && place.GetCover(map) == null && caves[place] <= 0f) {
                float num2 = map.fertilityGrid.FertilityAt(place);
                float num3 = num2 * desiredTotalDensity;
                if(Rand.Value < num3) {
                    for(int j = 0; j < plantDef.plant.wildClusterSizeRange.RandomInRange; j++) {
                        IntVec3 c2;
                        if(j == 0) {
                            c2 = place;
                        } else if(!GenPlantReproduction.TryFindReproductionDestination(place, plantDef, SeedTargFindMode.MapGenCluster, map, out c2)) {
                            break;
                        }
                        Plant plant = (Plant)ThingMaker.MakeThing(plantDef, null);
                        plant.Growth = Rand.Range(0.07f, 1f);
                        if(plant.def.plant.LimitedLifespan) {
                            plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
                        }
                        GenSpawn.Spawn(plant, c2, map);
                    }
                }
            }
        }
        private void generatePlants() {
            ThingDef plantDef = speciality.PlantSpeciality;
            if(plantDef == null) {
                return;
            }
            if(plantExtraSpawn <= 0) {
                return;
            }
            MapGenFloatGrid caves = MapGenerator.Caves;
            float desiredTotalDensity = map.Biome.plantDensity * map.gameConditionManager.AggregatePlantDensityFactor() * plantExtraSpawn;
            using(IEnumerator<IntVec3> enumerator = map.AllCells.InRandomOrder(null).GetEnumerator()) {
                while(enumerator.MoveNext()) {
                    IntVec3 c = enumerator.Current;
                    spawnPlant(c, caves, desiredTotalDensity, plantDef);
                }
            }
        }
        private void generateResourceRocks() {
            ThingDef resourceRockDef = speciality.ResourceRockSpeciality;
            if(resourceRockDef == null) {
                return;
            }
            if(resourceRockExtraSpawn <= 0) {
                return;
            }
            GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
            float num3 = 10f;
            switch(Find.WorldGrid[map.Tile].hilliness) {
            case Hilliness.Flat:
                num3 = 4f;
                break;
            case Hilliness.SmallHills:
                num3 = 8f;
                break;
            case Hilliness.LargeHills:
                num3 = 11f;
                break;
            case Hilliness.Mountainous:
                num3 = 15f;
                break;
            case Hilliness.Impassable:
                num3 = 16f;
                break;
            }
            num3 *= resourceRockExtraSpawn;
            genStep_ScatterLumpsMineable.forcedDefToScatter = resourceRockDef;
            genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num3, num3);
            genStep_ScatterLumpsMineable.Generate(map);
        }
    }
}
