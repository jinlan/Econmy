using System;
using System.Collections.Generic;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace RimEconomy {
    public class RimEconomy : ModBase {

        public static Dictionary<string, SettingHandle<string>> SettingData;

        public RimEconomy() : base() {
            SettingData = new Dictionary<string, SettingHandle<string>>();
        }

        public override string ModIdentifier {
            get {
                return "RimEconomy";
            }
        }
        public override void DefsLoaded() {
            float parsedFloat;
            SettingData["specialityChanceAnimal"] = Settings.GetHandle<string>("specialityChanceAnimal", "world animal speciality chance", "This is the chance of a world tile to gain an animal speciality.", SpecialityWorldManager.ChanceAnimal.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 0.8 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["specialityChancePlant"] = Settings.GetHandle<string>("specialityChancePlant", "world plant speciality chance", "This is the chance of a world tile to gain a plant speciality.", SpecialityWorldManager.ChancePlant.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 0.8 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["specialityChanceResourceRock"] = Settings.GetHandle<string>("specialityChanceResourceRock", "world resource rock speciality chance", "This is the chance of a world tile to gain an resource rock speciality.", SpecialityWorldManager.ChanceResourceRock.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 0.8 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["pawnExtraSpawn"] = Settings.GetHandle<string>("pawnExtraSpawn", "extra pawn", "How many pawns will be spawned in a pawn speciality map.", SpecialityMapManager.PawnExtraSpawn.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 5 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["plantExtraSpawn"] = Settings.GetHandle<string>("plantExtraSpawn", "extra plant", "How many plants will be spawned in a plant speciality map.", SpecialityMapManager.PlantExtraSpawn.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 1.5 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["resourceRockExtraSpawn"] = Settings.GetHandle<string>("resourceRockExtraSpawn", "extra resource rock", "How many resource rocks will be spawned in a resource rock speciality map.", SpecialityMapManager.ResourceRockExtraSpawn.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 10 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
        }
    }
}
