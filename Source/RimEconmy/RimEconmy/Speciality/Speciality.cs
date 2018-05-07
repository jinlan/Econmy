using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using HugsLib.Utils;

namespace RimEconmy {

    public class Speciality : ILoadReferenceable {

        private PawnKindDef animalSpeciality;
        private ThingDef plantSpeciality;
        private ThingDef resourceRockSpeciality;

        public PawnKindDef AnimalSpeciality {
            get {
                return animalSpeciality;
            }
            set {
                animalSpeciality = value;
                if(WorldObject != null) {
                    WorldObject.setAnimalKind(value);
                }
            }
        }


        public ThingDef PlantSpeciality {
            get {
                return plantSpeciality;
            }
            set {
                plantSpeciality = value;
                if(WorldObject != null) {
                    WorldObject.setPlant(value);
                }
            }
        }
        public ThingDef ResourceRockSpeciality {
            get {
                return resourceRockSpeciality;
            }
            set {
                resourceRockSpeciality = value;
                if(WorldObject != null) {
                    WorldObject.setResourceRock(value);
                }
            }
        }

        private SpecialityWorldObject WorldObject;
        private int tile;
        private List<ThingDef> allProductions;

        public Speciality(int tile, PawnKindDef animalBounus = null, ThingDef plantBounus = null, ThingDef resourceRockBounus = null) {
            this.tile = tile;
            SpecialityWorldObject worldObject = (SpecialityWorldObject)WorldObjectMaker.MakeWorldObject(SpecialityWorldObjectDefOf.Speciality);
            worldObject.Tile = tile;
            Find.WorldObjects.Add(worldObject);
            WorldObject = worldObject;
            AnimalSpeciality = animalBounus;
            PlantSpeciality = plantBounus;
            ResourceRockSpeciality = resourceRockBounus;
        }

        public List<ThingDef> getAllBounus() {
            List<ThingDef> allThingDef = new List<ThingDef>();
            if(animalSpeciality != null) {
                allThingDef.Add(animalSpeciality.race);
            }
            if(plantSpeciality != null) {
                allThingDef.Add(plantSpeciality);
            }
            if(resourceRockSpeciality != null) {
                allThingDef.Add(resourceRockSpeciality);
            }
            return allThingDef;
        }

        public List<ThingDef> getAllProductions() {
            if(allProductions == null) {
                allProductions = new List<ThingDef>();
                foreach(ThingDef specialityBounus in getAllBounus()) {
                    allProductions.AddRange(getProduction(specialityBounus));
                }
            }
            return allProductions;
        }

        public void ExposeData() {
            Scribe_Values.Look<int>(ref tile, "tile");
            Scribe_Defs.Look<PawnKindDef>(ref animalSpeciality, "ab");
            Scribe_Defs.Look<ThingDef>(ref plantSpeciality, "pb");
            Scribe_Defs.Look<ThingDef>(ref resourceRockSpeciality, "rr");
            Scribe_Deep.Look<SpecialityWorldObject>(ref WorldObject, "wo");
        }

        public string GetUniqueLoadID() {
            return "speciality" + tile.ToString();
        }

        private List<ThingDef> getProduction(ThingDef from) {
            List<ThingDef> allProduction = new List<ThingDef>();
            if(from.race != null) {
                if(from.race.meatDef != null) {
                    allProduction.Add(from.race.meatDef);
                }
                if(from.race.useMeatFrom != null) {
                    allProduction.Add(from.race.meatDef);
                }
                if(from.race.leatherDef != null) {
                    allProduction.Add(from.race.leatherDef);
                }
                CompProperties_Milkable milkProp = from.GetCompProperties<CompProperties_Milkable>();
                if(milkProp != null) {
                    allProduction.Add(milkProp.milkDef);
                }
                CompProperties_Shearable shearProp = from.GetCompProperties<CompProperties_Shearable>();
                if(shearProp != null) {
                    allProduction.Add(shearProp.woolDef);
                }
            }
            if(from.plant != null) {
                if(from.plant.harvestedThingDef != null) {
                    allProduction.Add(from.plant.harvestedThingDef);
                }
            }
            if(from.building != null) {
                if(from.building.mineableThing != null) {
                    allProduction.Add(from.building.mineableThing);
                }
            }
            return allProduction;
        }
    }
}
