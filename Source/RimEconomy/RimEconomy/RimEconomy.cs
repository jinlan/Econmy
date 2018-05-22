using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace RimEconomy {
    public class RimEconomy : ModBase {

        private const float ChanceAnimal = 0.0015f;
        private const float ChancePlant = 0.003f;
        private const float ChanceResourceRock = 0.00075f;

        private const float PawnExtraSpawn = 0.5f;
        private const float PlantExtraSpawn = 0.15f;
        private const float ResourceRockExtraSpawn = 1f;

        private const int ExtraPowerPerColonyLevel = 15;

        private const string GiddyUpCoreVersion = "0.18.7.0";
        private const string GiddyUpCaravanVersion = "0.18.1.0";

        public static Dictionary<string, SettingHandle<float>> SettingFloat;
        public static Dictionary<string, SettingHandle<int>> SettingInt;
        public static Dictionary<string, SettingHandle<string>> SettingString;

        public static Type GiddyUpCoreType;
        public static int GiddyUpCaravanBonus = 0;

        public RimEconomy() : base() {
            SettingFloat = new Dictionary<string, SettingHandle<float>>();
            SettingInt = new Dictionary<string, SettingHandle<int>>();
            SettingString = new Dictionary<string, SettingHandle<string>>();
        }

        public override string ModIdentifier {
            get {
                return "RimEconomy";
            }
        }
        public override void DefsLoaded() {
            SettingFloat["specialityChanceAnimal"] = Settings.GetHandle<float>("specialityChanceAnimal", "world animal speciality chance", "This is the chance of a world tile to gain an animal speciality.", ChanceAnimal, Validators.FloatRangeValidator(0, 0.8f));
            SettingFloat["specialityChancePlant"] = Settings.GetHandle<float>("specialityChancePlant", "world plant speciality chance", "This is the chance of a world tile to gain a plant speciality.", ChancePlant, Validators.FloatRangeValidator(0, 0.8f));
            SettingFloat["specialityChanceResourceRock"] = Settings.GetHandle<float>("specialityChanceResourceRock", "world resource rock speciality chance", "This is the chance of a world tile to gain an resource rock speciality.", ChanceResourceRock, Validators.FloatRangeValidator(0, 0.8f));
            SettingFloat["pawnExtraSpawn"] = Settings.GetHandle<float>("pawnExtraSpawn", "extra pawn", "How many pawns will be spawned in a pawn speciality map.", PawnExtraSpawn, Validators.FloatRangeValidator(0, 5));
            SettingFloat["plantExtraSpawn"] = Settings.GetHandle<float>("plantExtraSpawn", "extra plant", "How many plants will be spawned in a plant speciality map.", PlantExtraSpawn, Validators.FloatRangeValidator(0, 1.5f));
            SettingFloat["resourceRockExtraSpawn"] = Settings.GetHandle<float>("resourceRockExtraSpawn", "extra resource rock", "How many resource rocks will be spawned in a resource rock speciality map.", ResourceRockExtraSpawn, Validators.FloatRangeValidator(0, 10));
            SettingInt["extraFactionBasePowerPerSpeciality"] = Settings.GetHandle<int>("extraFactionBasePowerPerSpeciality", "extra faction base power per speciality", "Every speciality will be the nearby faction bases be generated more powerful.", ExtraPowerPerColonyLevel, Validators.FloatRangeValidator(0, 150));
            SettingString["GiddyUpCoreVersion"] = Settings.GetHandle<string>("GiddyUpCoreVersion", "version string of your installed GiddyUp Core!", "Version other than default is NOT guaranteed to work, but we can try :p.", GiddyUpCoreVersion);
            SettingString["GiddyUpCaravanVersion"] = Settings.GetHandle<string>("GiddyUpCaravanVersion", "version string of your installed GiddyUp Caravan!", "Version other than default is NOT guaranteed to work, but we can try :p.", GiddyUpCaravanVersion);

            checkGiddyUp();

        }
        protected void checkGiddyUp() {
            if(ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Giddy-up! Core") && ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Giddy-up! Caravan")) {
                string GiddyUpCoreDll = "GiddyUpCore, Version=" + SettingString["GiddyUpCoreVersion"].Value + ", Culture = neutral, PublicKeyToken = null";
                string GiddyUpCaravanDll = "GiddyUpCaravan, Version=" + SettingString["GiddyUpCaravanVersion"].Value + ", Culture = neutral, PublicKeyToken = null";
                GiddyUpCoreType = Type.GetType("GiddyUpCore.Utilities.IsMountableUtility" + ", " + GiddyUpCoreDll);
                Type typeCaravan = Type.GetType("GiddyUpCaravan.Base" + ", " + GiddyUpCoreDll);
                if(typeCaravan != null) {
                    GiddyUpCaravanBonus = ((SettingHandle<int>)(typeCaravan.GetField("completeCaravanBonus", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null))).Value;
                }
            }
        }
    }
}
