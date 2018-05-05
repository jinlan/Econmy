using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimEconmy {

    public class GenStep_Speciality : GenStep {

        public override void Generate(Map map) {
            SpecialityWorldManager specialitiesWorldManager = Find.World.GetComponent<SpecialityWorldManager>();
            Speciality speciality = specialitiesWorldManager.getSpeciality(map.Tile);
            if(speciality != null) {
                SpecialityMapManager specialityMapManager = map.GetComponent<SpecialityMapManager>();
                specialityMapManager.Generate(speciality);
            }
        }
    }
}
