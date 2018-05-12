using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace RimEconomy {

    public class WorldGenStep_Speciality : WorldGenStep {

        public override void GenerateFresh(string seed) {
            RimEconomyWorldManager rimEconomyWorldManager = Find.World.GetComponent<RimEconomyWorldManager>();
            rimEconomyWorldManager.generateSpecialitiesFresh(seed);
        }

        public override void GenerateFromScribe(string seed) {
            return;
        }
    }
}
