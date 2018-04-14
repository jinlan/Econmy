using System;
using System.Collections.Generic;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace RimEconmy {
    public class RimEconmy : ModBase {

        private const string Version = "0.1.0";

        public static Dictionary<string, SettingHandle<string>> SettingData;

        public RimEconmy() : base() {
            SettingData = new Dictionary<string, SettingHandle<string>>();
        }

        public override string ModIdentifier {
            get {
                return "RimEconmy";
            }
        }
        public override void DefsLoaded() {
            float useless;
            SettingData["specialityChance"] = Settings.GetHandle<string>("specialityChance", "world speciality chance", "This is the chance of a world tile to gain a speciality.", SpecialitiesWorldManager.Chance.ToString(), (string arg) => float.TryParse(arg, out useless));
            SettingData["pawnExtraSpawn"] = Settings.GetHandle<string>("pawnExtraSpawn", "extra pawn", "How many pawns will be spawned in a pawn speciality map.", SpecialityMapManager.PawnExtraSpawn.ToString(), (string arg) => float.TryParse(arg, out useless));
            SettingData["PlantExtraSpawn"] = Settings.GetHandle<string>("plantExtraSpawn", "extra plant", "How many plants will be spawned in a plant speciality map.", SpecialityMapManager.PlantExtraSpawn.ToString(), (string arg) => float.TryParse(arg, out useless));
            SettingData["ResourceRockExtraSpawn"] = Settings.GetHandle<string>("resourceRockExtraSpawn", "extra resource rock", "How many resource rocks will be spawned in a resource rock speciality map.", SpecialityMapManager.ResourceRockExtraSpawn.ToString(), (string arg) => float.TryParse(arg, out useless));
        }
    }
}
