using System;
using System.Collections.Generic;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace RimEconomy {
    public class RimEconomy : ModBase {

        private const float ChanceAnimal = 0.001f;
        private const float ChancePlant = 0.002f;
        private const float ChanceResourceRock = 0.0005f;

        private const float PawnExtraSpawn = 0.5f;
        private const float PlantExtraSpawn = 0.15f;
        private const float ResourceRockExtraSpawn = 1f;

        private const int ExtraPowerPerColonyLevel = 15;

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
            SettingData["specialityChanceAnimal"] = Settings.GetHandle<string>("specialityChanceAnimal", "world animal speciality chance", "This is the chance of a world tile to gain an animal speciality.", ChanceAnimal.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 0.8 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["specialityChancePlant"] = Settings.GetHandle<string>("specialityChancePlant", "world plant speciality chance", "This is the chance of a world tile to gain a plant speciality.", ChancePlant.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 0.8 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["specialityChanceResourceRock"] = Settings.GetHandle<string>("specialityChanceResourceRock", "world resource rock speciality chance", "This is the chance of a world tile to gain an resource rock speciality.", ChanceResourceRock.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 0.8 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["pawnExtraSpawn"] = Settings.GetHandle<string>("pawnExtraSpawn", "extra pawn", "How many pawns will be spawned in a pawn speciality map.", PawnExtraSpawn.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 5 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["plantExtraSpawn"] = Settings.GetHandle<string>("plantExtraSpawn", "extra plant", "How many plants will be spawned in a plant speciality map.", PlantExtraSpawn.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 1.5 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            SettingData["resourceRockExtraSpawn"] = Settings.GetHandle<string>("resourceRockExtraSpawn", "extra resource rock", "How many resource rocks will be spawned in a resource rock speciality map.", ResourceRockExtraSpawn.ToString(), (string arg) => {
                if(!float.TryParse(arg, out parsedFloat)) {
                    return false;
                }
                if(parsedFloat > 10 || parsedFloat < 0) {
                    return false;
                }
                return true;
            });
            int parsedInt;
            SettingData["extraFactionBasePowerPerSpeciality"] = Settings.GetHandle<string>("extraFactionBasePowerPerSpeciality", "extra faction base power per speciality", "Every speciality will be the nearby faction bases be generated more powerful.", ExtraPowerPerColonyLevel.ToString(), (string arg) => {
                if(!int.TryParse(arg, out parsedInt)) {
                    return false;
                }
                if(parsedInt > 150 || parsedInt < 0) {
                    return false;
                }
                return true;
            });
        }
    }
}
