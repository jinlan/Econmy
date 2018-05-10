using System;
using System.Text;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimEconmy {

    [StaticConstructorOnStartup]
    public class SpecialityWorldObject : WorldObject {

        private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        private string specialityLabels;

        private PawnKindDef animalKind;
        private ThingDef plantDef;
        private ThingDef resourceRockDef;

        private Material mat;

        public override Material Material {
            get {
                if(mat == null) {
                    if(animalKind != null) {
                        mat = animalKind.lifeStages[animalKind.lifeStages.Count - 1].bodyGraphicData.Graphic.MatSide;
                    } else if(plantDef != null) {
                        mat = plantDef.graphicData.Graphic.MatSingle;
                    } else if(resourceRockDef != null) {
                        mat = resourceRockDef.building.mineableThing.graphicData.Graphic.MatSingle;
                    }
                }
                return mat;
            }
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look<string>(ref specialityLabels, "sl");
            Scribe_Defs.Look<PawnKindDef>(ref animalKind, "ak");
            Scribe_Defs.Look<ThingDef>(ref plantDef, "pd");
            Scribe_Defs.Look<ThingDef>(ref resourceRockDef, "rrd");
        }

        public override void Draw() {
            float averageTileSize = Find.WorldGrid.averageTileSize;
            float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
            if(transitionPct > 0f) {
                Color color = Material.color;
                float num = 1f - transitionPct;
                SpecialityWorldObject.propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num));
                Vector3 drawPos = DrawPos;
                float size = 0.7f * averageTileSize;
                float altOffset = 0.015f;
                Material material = Material;
                MaterialPropertyBlock materialPropertyBlock = SpecialityWorldObject.propertyBlock;
                WorldRendererUtility.DrawQuadTangentialToPlanet(drawPos, size, altOffset, material, false, true, materialPropertyBlock);
            } else {
                WorldRendererUtility.DrawQuadTangentialToPlanet(DrawPos, 0.7f * averageTileSize, 0.015f, Material, false, false, null);
            }

        }

        public override string GetDescription() {
            StringBuilder sb = new StringBuilder(def.description);
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(specialityLabels);
            return sb.ToString();
        }

        public override string GetInspectString() {
            return specialityLabels;
        }

        public void setPlant(ThingDef plant) {
            this.plantDef = plant;
            onChange();
        }

        public void setAnimalKind(PawnKindDef animalKind) {
            this.animalKind = animalKind;
            onChange();
        }
        public void setResourceRock(ThingDef resourceRock) {
            this.resourceRockDef = resourceRock;
            onChange();
        }

        private void onChange() {
            List<string> labels = new List<string>(3);
            if(animalKind != null) {
                labels.Add(animalKind.label);
            }
            if(plantDef != null) {
                labels.Add(plantDef.label);
            }
            if(resourceRockDef != null) {
                labels.Add(resourceRockDef.label);
            }
            StringBuilder sb = new StringBuilder();
            foreach(string label in labels) {
                sb.Append(label);
                sb.Append(',');
                sb.Append(' ');
            }
            int startIndex = Math.Max(sb.Length - 2, 0);
            sb.Remove(startIndex, Math.Min(2, sb.Length - startIndex));
            specialityLabels = sb.ToString();
        }
    }
}
