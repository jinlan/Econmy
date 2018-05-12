using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimEconomy {

    public static class PublicExtension {

        public static float DesiredAnimalDensity(this WildSpawner wildSpawner, Map map) {
            float num = map.Biome.animalDensity * 2;
            float num2 = 0f;
            float num3 = 0f;
            foreach(PawnKindDef pawnKindDef in map.Biome.AllWildAnimals) {
                num3 += pawnKindDef.wildSpawn_EcoSystemWeight;
                if(map.mapTemperature.SeasonAcceptableFor(pawnKindDef.race)) {
                    num2 += pawnKindDef.wildSpawn_EcoSystemWeight;
                }
            }
            num *= num2 / num3;
            num *= map.GameConditionManager.AggregateAnimalDensityFactor();
            return num;
        }

        public static float DesiredTotalAnimalWeight(this WildSpawner wildSpawner, Map map) {
            float desiredAnimalDensity = wildSpawner.DesiredAnimalDensity(map);
            if(!(desiredAnimalDensity > 0f) && !(desiredAnimalDensity < 0f)) {
                return 0f;
            }
            float num = 10000f / desiredAnimalDensity;
            return map.Area / num;
        }

        public static float CurrentTotalAnimalWeight(this WildSpawner wildSpawner, Map map) {
            float num = 0f;
            List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
            for(int i = 0; i < allPawnsSpawned.Count; i++) {
                if(allPawnsSpawned[i].kindDef.wildSpawn_spawnWild && allPawnsSpawned[i].Faction == null) {
                    num += allPawnsSpawned[i].kindDef.wildSpawn_EcoSystemWeight;
                }
            }
            return num;
        }

        public static float AggregateAnimalDensityFactor(this GameConditionManager gcm) {
            float num = 1f;
            for(int i = 0; i < gcm.ActiveConditions.Count; i++) {
                num *= gcm.ActiveConditions[i].AnimalDensityFactor();
            }
            if(gcm.Parent != null) {
                num += AggregateAnimalDensityFactor(gcm.Parent);
            }
            return num;
        }

        public static float AggregatePlantDensityFactor(this GameConditionManager gcm) {
            float num = 1f;
            for(int i = 0; i < gcm.ActiveConditions.Count; i++) {
                num *= gcm.ActiveConditions[i].PlantDensityFactor();
            }
            if(gcm.Parent != null) {
                num += AggregatePlantDensityFactor(gcm.Parent);
            }
            return num;
        }

        public class CompareFloat : IComparer<float> {
            int IComparer<float>.Compare(float x, float y) {
                if(x > y) return 1;
                else if(x < y) return -1;
                else return 0;
            }
        }
        public class randomOrder<T> : IComparer<T> {
            private readonly Dictionary<T, float> randomValues = new Dictionary<T, float>();
            private readonly IComparer<float> floatComparer = new CompareFloat();
            int IComparer<T>.Compare(T x, T y) {
                if(!randomValues.ContainsKey(x)) {
                    randomValues[x] = Rand.Value;
                }
                if(!randomValues.ContainsKey(y)) {
                    randomValues[y] = Rand.Value;
                }
                return floatComparer.Compare(randomValues[x], randomValues[y]);
            }
        }
    }
}
