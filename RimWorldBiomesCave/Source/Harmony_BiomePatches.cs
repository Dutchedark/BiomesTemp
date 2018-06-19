using Harmony;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System;
using Verse.AI;

namespace RimWorldBiomesCave
{
    [StaticConstructorOnStartup]
    static class Harmony_BiomePatches
    {
        static Harmony_BiomePatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.SoggyNoodle.biomepatches");


            harmony.Patch(AccessTools.Method(typeof(Mineable), "TrySpawnYield"), null, new HarmonyMethod(typeof(Harmony_BiomePatches), nameof(TrySpawnYield_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_GetFood), "TryGiveJob"), null, new HarmonyMethod(typeof(Harmony_BiomePatches), nameof(TryGiveJob_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(PlantProperties), "get_IsTree"), null, new HarmonyMethod(typeof(Harmony_BiomePatches), nameof(Get_IsTree_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(GenPlant), "CanEverPlantAt"), null, new HarmonyMethod(typeof(Harmony_BiomePatches), nameof(CanEverPlantAt_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(GenStep_ScatterLumpsMineable), "ScatterAt"), new HarmonyMethod(typeof(Harmony_BiomePatches), nameof(ScatterAt_PreFix)), null);
        }
       
        public static bool ScatterAt_PreFix(IntVec3 c, Map map, GenStep_ScatterLumpsMineable __instance){

            ThingDef thingDef = null;
            if(__instance.forcedDefToScatter != null){
                thingDef = __instance.forcedDefToScatter;
            }
            else{
                thingDef = DefDatabase<ThingDef>.AllDefs.RandomElementByWeight(delegate (ThingDef d)
                {
                    if (d.building == null)
                    {
                        return 0f;
                    }
                    if(d.GetCompProperties<CompProperties_BiomeSpecific>() != null && !d.GetCompProperties<CompProperties_BiomeSpecific>().allowedBiomes.Contains(map.Biome.defName)){
                        return 0f;
                    }
                    return d.building.mineableScatterCommonality;
                });
            }

            List<IntVec3> recentLumpCells = (List<IntVec3>)AccessTools.Field(typeof(GenStep_ScatterLumpsMineable), "recentLumpCells").GetValue(__instance);
            int numCells = (__instance.forcedLumpSize <= 0) ? thingDef.building.mineableScatterLumpSizeRange.RandomInRange : __instance.forcedLumpSize;
            recentLumpCells.Clear();
            foreach (IntVec3 current in GridShapeMaker.IrregularLump(c, map, numCells))
            {
                GenSpawn.Spawn(thingDef, current, map);
                recentLumpCells.Add(current);
            }
            AccessTools.Field(typeof(GenStep_ScatterLumpsMineable), "recentLumpCells").SetValue(__instance,recentLumpCells);
            return false;
          
        }

        public static void CanEverPlantAt_PostFix(ThingDef plantDef, IntVec3 c, Map map, ref bool __result){
            if(__result == false && plantDef.GetCompProperties<CompProperties_WaterPlant>() != null){
                if(plantDef.GetCompProperties<CompProperties_WaterPlant>().allowedTiles.Contains(c.GetTerrain(map))){
                    __result = true;
                }
            }
            
        }

        public static void TryGiveJob_PostFix(JobGiver_GetFood __instance, ref Job __result, Pawn pawn){
            if(__result?.targetA.Thing != null && (__result?.targetA.Thing as Corpse != null || __result?.targetA.Thing as Pawn != null && ((Pawn)__result?.targetA.Thing).Downed)){
                return;
            }
            if(__result?.def == JobDefOf.PredatorHunt && pawn?.GetComp<CompVampire>() != null){
                __result = new Job(RWBDefOf.RWBVampireBite, __result.targetA);
            }
        }

        public static void TrySpawnYield_PostFix(Map map, float yieldChance, bool moteOnWaste, Mineable __instance){
            if(__instance.def.defName == "RWBStalagmite"){
                IntVec3 current = __instance.Position;
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

                int R = Rand.RangeInclusive(0, 100);
                if (R < 50 && thing != "")
                {
                    GenSpawn.Spawn(ThingDef.Named(thing), current, map);
                }

            }
        }
 
        public static void Get_IsTree_PostFix(PlantProperties __instance, ref bool __result)
        {
            if(__instance.harvestTag == "FungalLog"){
                __result = true;
            }
        }
    }
}
