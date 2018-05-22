using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace RimEconmy {

    public class WorldGenStep_Speciality : WorldGenStep {

        public override void GenerateFresh(string seed) {
            SpecialityWorldManager mapBounusWorldManager = Find.World.GetComponent<SpecialityWorldManager>();
            mapBounusWorldManager.generateFresh(seed);
        }

        public override void GenerateFromScribe(string seed) {
            return;
        }
    }
}
