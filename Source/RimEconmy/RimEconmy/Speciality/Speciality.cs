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

        public SpecialityWorldObject WorldObject;
        private int tile;

        public Speciality(int tile, PawnKindDef animalBounus = null, ThingDef plantBounus = null, ThingDef resourceRockBounus = null) {
            this.tile = tile;
            if(!Find.WorldObjects.AnyWorldObjectAt(tile)) {
                SpecialityWorldObject worldObject = (SpecialityWorldObject)WorldObjectMaker.MakeWorldObject(SpecialityWorldObjectDefOf.Speciality);
                worldObject.Tile = tile;
                Find.WorldObjects.Add(worldObject);
                WorldObject = worldObject;
            }
            AnimalSpeciality = animalBounus;
            PlantSpeciality = plantBounus;
            ResourceRockSpeciality = resourceRockBounus;
        }

        public void ExposeData() {
            Scribe_Values.Look<int>(ref tile, "tile");
            Scribe_Defs.Look<PawnKindDef>(ref animalSpeciality, "ab");
            Scribe_Defs.Look<ThingDef>(ref plantSpeciality, "pb");
            Scribe_Defs.Look<ThingDef>(ref resourceRockSpeciality, "rr");
            Scribe_References.Look<SpecialityWorldObject>(ref WorldObject, "wo");
        }

        public string GetUniqueLoadID() {
            return tile.ToString();
        }
    }
}
