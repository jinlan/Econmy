using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using HugsLib.Utils;

namespace RimEconmy {
    public class StockGenerator_RimEconmy : StockGenerator {

        private List<Thing> productionListWithoutQuantity;

        public override IEnumerable<Thing> GenerateThings(int forTile) {
            reset();
            foreach(Thing thing in productionListWithoutQuantity) {
                ThingDef thingDef = thing.def;
                if(thing is Pawn) {
                    PawnGenerationRequest req = new PawnGenerationRequest((thing as Pawn).kindDef, null, PawnGenerationContext.NonPlayer, forTile);
                    for(int i = 1; i <= base.RandomCountOf(thingDef); i++) {
                        yield return PawnGenerator.GeneratePawn(req);
                    }
                } else if(thingDef.MadeFromStuff) {
                    for(int i = 1; i <= base.RandomCountOf(thingDef); i++) {
                        yield return ThingMaker.MakeThing(thingDef, thing.Stuff);
                    }
                } else {
                    thing.stackCount = base.RandomCountOf(thingDef);                    yield return thing;
                }
            }
            productionListWithoutQuantity = null;
            yield break;
        }

        public override bool HandlesThingDef(ThingDef thingDef) {
            reset();
            return productionListWithoutQuantity.Any((Thing obj) => obj.def == thingDef);
        }

        private void reset() {
            if(productionListWithoutQuantity == null) {
                List<Thing> fullProductionList = ProductionWorldManager.singleton.getProductionList();
                productionListWithoutQuantity = fullProductionList.GetRange(0, Math.Max(Math.Min(5, fullProductionList.Count), (int)(Rand.Value * fullProductionList.Count)));
                productionListWithoutQuantity.AddRange(ProductionWorldManager.singleton.getRawMaterials().ConvertAll((ThingDef input) => {
                    if(input.race != null) {
                        Speciality speciality = ProductionWorldManager.singleton.getSpecialityList().Find((Speciality obj) => obj.AnimalSpeciality != null && obj.AnimalSpeciality.race == input);
                        return PawnGenerator.GeneratePawn(speciality.AnimalSpeciality);
                    } else {
                        return ThingMaker.MakeThing(input);
                    }
                }));
            }
            Settlement settlement = TradeSession.trader as Settlement;
            if(settlement != null) {
                maxTechLevelGenerate = settlement.Faction.def.techLevel;
            }
            int countBounus = ProductionWorldManager.singleton.getSpecialityList().Aggregate(0, (int count, Speciality speciality) => count + speciality.getAllBounus().Count);
            if(totalPriceRange == FloatRange.Zero) {
                totalPriceRange = new FloatRange(1000 * countBounus, 2000 * countBounus);
            }
        }
    }
}
