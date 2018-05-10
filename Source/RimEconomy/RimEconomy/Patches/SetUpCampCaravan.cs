using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.Planet;
using HugsLib.Utils;

namespace RimEconomy.Patches.SetUpCamp {
    public class SetUpCampCaravan : Caravan {

        private Gizmo setUpCampCommand;

        public SetUpCampCaravan(){
            Type type = Type.GetType("Nandonalt_SetUpCamp.CaravanCampUtility, Nandonalt_SetUpCamp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            setUpCampCommand = (Gizmo)type.GetMethod("CampCommand", BindingFlags.Static | BindingFlags.Public).Invoke(null, BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder, new object[]{this}, null);
        }

        public override IEnumerable<Gizmo> GetGizmos() {
            StringBuilder failReason = new StringBuilder();
            if(!TileFinder.IsValidTileForNewSettlement(this.Tile, failReason)) {
                setUpCampCommand.Disable(failReason.ToString());
            }else{
                setUpCampCommand.disabled = false;
                setUpCampCommand.disabledReason = null;
            }
            List<Gizmo> list = base.GetGizmos().ToList();
            list.Add(setUpCampCommand);
            return list;
        }
    }
}
