using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace RimEconmy {

    public class WorldGenStep_RimEconmy : WorldGenStep {

        public override void GenerateFresh(string seed) {
            SpecialitiesWorldManager mapBounusWorldManager = Find.World.GetComponent<SpecialitiesWorldManager>();
            mapBounusWorldManager.generateFresh(seed);
        }

        public override void GenerateFromScribe(string seed) {
            return;
        }
    }
}
