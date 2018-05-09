using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Nandonalt_SetUpCamp;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.Planet;
using HugsLib.Utils;

namespace RimEconmy.Patches.SetUpCamp {
    public class SetUpCampCaravan : CaravanModded {
        public override IEnumerable<Gizmo> GetGizmos() {
            List<Gizmo> list = new List<Gizmo>();
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "Set-Up Camp";
            command_Action.defaultDesc = "Set-Up Camp";
            command_Action.icon = SetUpCampTextures.CampCommandTex;
            command_Action.action = delegate {
                SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                CaravanCampUtility.Camp(this);
            };
            StringBuilder failReason = new StringBuilder();
            if(!TileFinder.IsValidTileForNewSettlement(this.Tile, failReason)) {
                command_Action.Disable(failReason.ToString());
            }
            list.Add(command_Action);
            return base.GetGizmos().Concat(list);
        }
    }
}
