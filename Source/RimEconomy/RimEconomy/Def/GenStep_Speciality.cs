using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimEconomy {

    public class GenStep_Speciality : GenStep {

        public override void Generate(Map map) {
            RimEconomyWorldManager specialitiesWorldManager = Find.World.GetComponent<RimEconomyWorldManager>();
            Speciality speciality = specialitiesWorldManager.getTileSpeciality(map.Tile);
            if(speciality != null) {
                RimEconomyMapManager specialityMapManager = map.GetComponent<RimEconomyMapManager>();
                specialityMapManager.GenerateMap(speciality);
            }
        }
    }
}
