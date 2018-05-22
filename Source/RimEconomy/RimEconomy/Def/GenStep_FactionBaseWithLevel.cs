using System;
using RimWorld;
using Verse;
using RimWorld.BaseGen;

namespace RimEconomy {
    public class GenStep_FactionBaseWithLevel : GenStep_FactionBase {

        protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1) {
            RimEconomyWorldManager rimEconomyWorldManager = Find.World.GetComponent<RimEconomyWorldManager>();
            int specialityCount = rimEconomyWorldManager.getSettlementTileSpecialities(map.Tile).Count;
            int extraFactionBasePowerPerSpeciality = RimEconomy.SettingInt["extraFactionBasePowerPerSpeciality"].Value;
            float Modifier = 1;
            IntRange factionBaseSizeRange = new IntRange(Math.Min(map.Size.x - 50, 34 + (int)(specialityCount * extraFactionBasePowerPerSpeciality * Modifier)), Math.Min(map.Size.z - 50, 38 + (int)(specialityCount * extraFactionBasePowerPerSpeciality * Modifier)));
            int randomInRange = factionBaseSizeRange.RandomInRange;
            int randomInRange2 = factionBaseSizeRange.RandomInRange;
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            Faction faction;
            if(map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer) {
                faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            } else {
                faction = map.ParentFaction;
            }
            rect.ClipInsideMap(map);
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = rect;
            resolveParams.faction = faction;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push("factionBase", resolveParams);
            BaseGen.Generate();
        }
    }
}
