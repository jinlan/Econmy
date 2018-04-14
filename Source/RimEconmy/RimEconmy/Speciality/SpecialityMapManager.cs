using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace RimEconmy {

    public class SpecialityMapManager : MapComponent {

        public const float PawnExtraSpawn = 0.5f;
        public const float PlantExtraSpawn = 0.15f;
        public const float ResourceRockExtraSpawn = 1f;

        private Speciality speciality;

        public SpecialityMapManager(Map map) : base(map) {
        }

        public void Generate(Speciality speciality) {
            this.speciality = speciality;
            spawnAnimal();
            map.regionAndRoomUpdater.Enabled = false;
            spawnPlant();
            spawnResourceRock();
            map.regionAndRoomUpdater.Enabled = true;
        }

        private void spawnAnimal() {
            PawnKindDef animalKingDef = speciality.AnimalSpeciality;
            if(animalKingDef == null) {
                return;
            }
            if(!map.mapTemperature.SeasonAcceptableFor(animalKingDef.race)) {
                return;
            }
            float pawnExtraSpawn;
            if(!float.TryParse(RimEconmy.SettingData["pawnExtraSpawn"].Value, out pawnExtraSpawn)) {
                pawnExtraSpawn = PawnExtraSpawn;
            }
            float desiredTotalWeight = map.wildSpawner.DesiredTotalAnimalWeight(map) * (1 + pawnExtraSpawn);
            float currentWeight = map.wildSpawner.CurrentTotalAnimalWeight(map);
            int randomInRange = animalKingDef.wildSpawn_GroupSizeRange.RandomInRange;
            while(currentWeight <= desiredTotalWeight) {
                List<Pawn> pawns = new List<Pawn>();
                IntVec3 loc = RCellFinder.RandomAnimalSpawnCell_MapGen(map);
                int radius = Mathf.CeilToInt(Mathf.Sqrt((float)animalKingDef.wildSpawn_GroupSizeRange.max));
                for(int i = 0; i <= randomInRange; i++) {
                    IntVec3 loc2 = CellFinder.RandomClosewalkCellNear(loc, map, radius, null);
                    Pawn newAnimal = PawnGenerator.GeneratePawn(animalKingDef, null);
                    GenSpawn.Spawn(newAnimal, loc2, map);
                    pawns.Add(newAnimal);
                }
                currentWeight += (randomInRange + 1) * animalKingDef.wildSpawn_EcoSystemWeight;
            }
        }
        private void spawnPlant() {
            ThingDef plantDef = speciality.PlantSpeciality;
            if(plantDef == null) {
                return;
            }
            float plantExtraSpawn;
            if(!float.TryParse(RimEconmy.SettingData["plantExtraSpawn"].Value, out plantExtraSpawn)) {
                plantExtraSpawn = PlantExtraSpawn;
            }
            MapGenFloatGrid caves = MapGenerator.Caves;
            float desiredTotalDensity = map.Biome.plantDensity * map.gameConditionManager.AggregatePlantDensityFactor() * plantExtraSpawn;
            using(IEnumerator<IntVec3> enumerator = map.AllCells.InRandomOrder(null).GetEnumerator()) {
                while(enumerator.MoveNext()) {
                    IntVec3 c = enumerator.Current;
                    if(c.GetEdifice(map) == null && c.GetCover(map) == null && caves[c] <= 0f) {
                        float num2 = map.fertilityGrid.FertilityAt(c);
                        float num3 = num2 * desiredTotalDensity;
                        if(Rand.Value < num3) {
                            int randomInRange = plantDef.plant.wildClusterSizeRange.RandomInRange;
                            for(int j = 0; j < randomInRange; j++) {
                                IntVec3 c2;
                                if(j == 0) {
                                    c2 = c;
                                } else if(!GenPlantReproduction.TryFindReproductionDestination(c, plantDef, SeedTargFindMode.MapGenCluster, map, out c2)) {
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
            }
        }
        private void spawnResourceRock() {
            ThingDef resourceRockDef = speciality.ResourceRockSpeciality;
            if(resourceRockDef == null) {
                return;
            }
            float resourceRockExtraSpawn;
            if(!float.TryParse(RimEconmy.SettingData["resourceRockExtraSpawn"].Value, out resourceRockExtraSpawn)) {
                resourceRockExtraSpawn = ResourceRockExtraSpawn;
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
