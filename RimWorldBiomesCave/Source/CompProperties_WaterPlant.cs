using Harmony;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System;

namespace RimWorldBiomesCave
{
    [StaticConstructorOnStartup]
    public class CompProperties_WaterPlant : CompProperties
    {
        private const float V = 0.1f;
        public List<TerrainDef> allowedTiles = new List<TerrainDef>();
        public float spawnChance = V;
        public List<String> allowedBiomes = new List<String>();
        public bool growNearWater = false;
		public CompProperties_WaterPlant()
		{

			this.compClass = typeof(CompWaterPlant);
		}
    }


}
