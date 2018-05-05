using System;
using System.Collections.Generic;
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
                if(thingDef.stackLimit == 1) {
                    yield return thing;
                } else {
                    int count = base.RandomCountOf(thingDef);
                    if(thing is Pawn) {
                        PawnGenerationRequest req = new PawnGenerationRequest((thing as Pawn).kindDef, null, PawnGenerationContext.NonPlayer, forTile);
                        for(int i = 1; i <= count; i++) {
                            yield return PawnGenerator.GeneratePawn(req);
                        }
                    } else {
                        thing.stackCount = count;
                        yield return thing;
                    }
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
                ModLogger logger = new ModLogger("production full:");
                logger.Trace(fullProductionList);
                foreach(Thing t in fullProductionList) {
                    logger.Trace(t);
                }
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
                ModLogger logger = new ModLogger("production randomized: " + settlement.TraderName);
                logger.Trace(productionListWithoutQuantity);
                foreach(Thing t in productionListWithoutQuantity) {
                    logger.Trace(t);
                }
            }
            if(totalPriceRange == FloatRange.Zero) {
                totalPriceRange = new FloatRange(1000 * ProductionWorldManager.singleton.getSpecialityList().Count, 2000 * ProductionWorldManager.singleton.getSpecialityList().Count);
            }
        }
    }
}
