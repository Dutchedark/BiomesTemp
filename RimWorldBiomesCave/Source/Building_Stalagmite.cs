using System;
using RimWorld;
using Verse;

namespace RimWorldBiomesCave
{
    [StaticConstructorOnStartup]
    public class Building_Stalagmite : Mineable
    {
        public bool first = false;
        private const int arraySize = 4;
        public static Graphic[] graphic = null;
        private string graphicPathAdditionWoNumber = "_style";
        public Graphic graph = null;
        private void UpdateGraphics()
        {
            int indexOf = def.graphicData.texPath.ToLower().LastIndexOf(graphicPathAdditionWoNumber);
            string graphicRealPath = def.graphicData.texPath.Remove(indexOf);

            while (graph == null)
            {
                graph = GraphicDatabase.Get<Graphic_Single>(graphicRealPath + graphicPathAdditionWoNumber + Rand.RangeInclusive(1, arraySize).ToString(), def.graphic.Shader, def.graphic.drawSize, base.Position.GetTerrain(base.Map).color, def.graphic.ColorTwo, def.graphic.data);
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (!first && graph == null)
                {
                    UpdateGraphics();
                    first = true;
                }
                IntVec3 current = base.Position;
                Map map = base.Map;
                return graph;
            }
        }

        public override void Destroy(DestroyMode mode)
        {

            IntVec3 current = base.Position;
            Map map = base.Map;
            String thing = "";
            if (current.GetTerrain(map).defName.Contains("Sandstone"))
            {
                thing = "ChunkSandstone";
            }
            if (current.GetTerrain(map).defName.Contains("Marble"))
            {
                thing = "ChunkMarble";
            }
            if (current.GetTerrain(map).defName.Contains("Slate"))
            {
                thing = "ChunkSlate";
            }
            if (current.GetTerrain(map).defName.Contains("Granite"))
            {
                thing = "ChunkGranite";
            }
            if (current.GetTerrain(map).defName.Contains("Limestone"))
            {
                thing = "ChunkLimestone";
            }

            base.Destroy(mode);
            int R = Rand.RangeInclusive(0, 100);
            if (R < 50 && thing != "")
            {
                GenSpawn.Spawn(ThingDef.Named(thing), current, map);
            }

        }
    }
}       
