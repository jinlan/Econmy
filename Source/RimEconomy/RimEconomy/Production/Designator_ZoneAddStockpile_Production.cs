using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace RimEconmy {
    public class Designator_ZoneAddStockpile_Production : Designator_ZoneAddStockpile_Resources {
        public Designator_ZoneAddStockpile_Production() {
            this.preset = StorageSettingsPreset.DefaultStockpile;
            this.defaultLabel = "ZoneAddProductions".Translate();
            this.defaultDesc = "ZoneAddProductionsDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Commands/Trade", true);
            this.hotKey = KeyBindingDefOf.Misc1;
            this.tutorTag = "ZoneAddProductions";
        }
    }
}
