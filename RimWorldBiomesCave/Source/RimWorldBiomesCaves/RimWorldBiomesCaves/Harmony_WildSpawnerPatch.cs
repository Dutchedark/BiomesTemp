using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RimWorld;
using UnityEngine;
using Harmony;
using Verse;
using System.Reflection;

namespace RimWorldBiomesCaves
{
    /*[StaticConstructorOnStartup]
    public static class Harmony_WildSpawnerPatch{
        static Harmony_WildSpawnerPatch(){
            Log.Message("Performing Wild Plant Growth Magic");
            HarmonyInstance Harmony = HarmonyInstance.Create("rimworld.soggynoodle.wildspawnerpatch");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    */
    [StaticConstructorOnStartup]
    static class Harmony_WildSpawnerPatch
    {

        static Harmony_WildSpawnerPatch()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.soggynoodle.wildspawnerpatch");

            harmony.Patch(AccessTools.Method(typeof(WildSpawner), "TrySpawnPlantFromMapEdge"), null,
                new HarmonyMethod(typeof(Harmony_WildSpawnerPatch), nameof(WildSpawner_TrySpawnPlantFromMapEdge_PostFix)), null);

        }

        private static void WildSpawner_TrySpawnPlantFromMapEdge_PostFix(WildSpawner __instance)
        {
            //every 2 in game seconds at speed 1
            if ((Find.TickManager.TicksGame % 60) == 0)
            {
                Log.Message("tick tack");
                float SpawnedMaturity = 0.05f;
                int SpawnRate = 1;

                //are we in the caverns
                Map map = (Map)typeof(WildSpawner).GetField("map", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                if (map.Biome.defName == "RWBCavern")
                {
                    Log.Message("Biome = RWBCAVERN");
                    //at normal spawnrate of 1
                    ThingDef plantDef;
                    if (SpawnRate == 1)
                    {
                        if (!map.Biome.AllWildPlants.TryRandomElementByWeight((ThingDef def) => map.Biome.CommonalityOfPlant(def), out plantDef))
                        {
                            return;
                        }
                        // Checks wether the plantdef has a fertility value(Added for TiberiumRim users since Tiberium has 0% fertility)
                        if (plantDef.fertility != 0)
                        {
                            IntVec3 source = CellFinder.RandomCell(map);
                            if (plantDef.CanEverPlantAt(source, map))
                            {
                                //insert the seed
                                Log.Message(source.ToString());
                                GenPlantReproduction.TryReproduceInto(source, plantDef, map);
                                if (source.GetPlant(map).def == plantDef)
                                {
                                    Log.Message("planted " + plantDef);
                                    source.GetPlant(map).Growth = SpawnedMaturity;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}