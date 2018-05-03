﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;
using RimWorld.Planet;
using RimWorld.BaseGen;
using System.Text;
using System;
using Verse.Sound;
using Verse.AI.Group;
using Verse.Noise;
namespace RimWorldBiomesCaves
{
    public class GenStep_Cavern : GenStep
    {
        private ModuleBase directionNoise;

        private static HashSet<IntVec3> tmpGroupSet = new HashSet<IntVec3>();

        private const float OpenTunnelsPer10k = 10.6f;

        private const float ClosedTunnelsPer10k = 5.0f;

        private const int MaxOpenTunnelsPerRockGroup = 15;

        private const int MaxClosedTunnelsPerRockGroup = 5;

        private const float DirectionChangeSpeed = 8f;

        private const float DirectionNoiseFrequency = 0.00205f;

        private const int MinRocksToGenerateAnyTunnel = 300;

        private const int AllowBranchingAfterThisManyCells = 20;

        private const float MinTunnelWidth = 3.0f;

        private const float WidthOffsetPerCell = 0.034f;

        private const float BranchChance = 0.6f;

        private static readonly FloatRange BranchedTunnelWidthOffset = new FloatRange(0.2f, 0.4f);

        private static readonly SimpleCurve TunnelsWidthPerRockCount = new SimpleCurve
        {
            {
                new CurvePoint(100f, 5f),
                true
            },
            {
                new CurvePoint(300f, 6f),
                true
            },
            {
                new CurvePoint(3000f, 7f),
                true
            }
        };

        private static List<IntVec3> tmpCells = new List<IntVec3>();

        private static HashSet<IntVec3> groupSet = new HashSet<IntVec3>();

        private static HashSet<IntVec3> groupVisited = new HashSet<IntVec3>();

        private static List<IntVec3> subGroup = new List<IntVec3>();

        public override void Generate(Map map)
        {
            //if (!Find.World.HasCaves(map.Tile))
            //{
            //    return;
            //}
            this.directionNoise = new Perlin(0.0020500000100582838, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            BoolGrid visited = new BoolGrid(map);
            List<IntVec3> group = new List<IntVec3>();
            foreach (IntVec3 current in map.AllCells)
            {
                if (!visited[current] && this.IsRock(current, elevation, map))
                {
                    group.Clear();
                    map.floodFiller.FloodFill(current, (IntVec3 x) => this.IsRock(x, elevation, map), delegate(IntVec3 x)
                    {
                        visited[x] = true;
                        group.Add(x);
                    }, 2147483647, false, null);
                    this.Trim(group, map);
                    //this.RemoveSmallDisconnectedSubGroups(group, map);
                    if (group.Count >= 50)
                    {
                        this.DoOpenTunnels(group, map);
                        this.DoClosedTunnels(group, map);
                    }
                }
            }
        }

        private void Trim(List<IntVec3> group, Map map)
        {
            GenMorphology.Open(group, 6, map);
        }

        private bool IsRock(IntVec3 c, MapGenFloatGrid elevation, Map map)
        {
            return c.InBounds(map) && elevation[c] > 0.7f;
        }

        private void DoOpenTunnels(List<IntVec3> group, Map map)
        {
            int num = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * 800f / 10000f);
            num = Mathf.Min(num, 40);
            if (num > 0)
            {
                num = Rand.RangeInclusive(20, num);
            }
            float num2 = GenStep_Cavern.TunnelsWidthPerRockCount.Evaluate((float)group.Count);
            for (int i = 0; i < num; i++)
            {
                IntVec3 start = IntVec3.Invalid;
                float num3 = -1f;
                float dir = -1f;
                float num4 = -1f;
                for (int j = 0; j < 40; j++)
                {
                    IntVec3 intVec = this.FindRandomEdgeCellForTunnel(group, map);
                    float distToCave = this.GetDistToCave(intVec, group, map, 40f, false);
                    float num6;
                    float num5 = this.FindBestInitialDir(intVec, group, out num6);
                    if (!start.IsValid || distToCave > num3 || (distToCave == num3 && num6 > num4))
                    {
                        start = intVec;
                        num3 = distToCave;
                        dir = num5;
                        num4 = num6;
                    }
                }
                float width = Rand.Range(num2 * 0.8f, num2);
                this.Dig(start, dir, width, group, map, false, null);
            }
        }

        private void DoClosedTunnels(List<IntVec3> group, Map map)
        {
            int num = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * 200f / 10000f);
            num = Mathf.Min(num, 20);
            if (num > 0)
            {
                num = Rand.RangeInclusive(10, num);
            }
            float num2 = GenStep_Cavern.TunnelsWidthPerRockCount.Evaluate((float)group.Count);
            for (int i = 0; i < num; i++)
            {
                IntVec3 start = IntVec3.Invalid;
                float num3 = -1f;
                for (int j = 0; j < 7; j++)
                {
                    IntVec3 intVec = group.RandomElement<IntVec3>();
                    float distToCave = this.GetDistToCave(intVec, group, map, 30f, true);
                    if (!start.IsValid || distToCave > num3)
                    {
                        start = intVec;
                        num3 = distToCave;
                    }
                }
                float width = Rand.Range(num2 * 0.8f, num2);
                this.Dig(start, Rand.Range(0f, 360f), width, group, map, true, null);
            }
        }

        private IntVec3 FindRandomEdgeCellForTunnel(List<IntVec3> group, Map map)
        {
            MapGenFloatGrid caves = MapGenerator.Caves;
            IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
            GenStep_Cavern.tmpCells.Clear();
            GenStep_Cavern.tmpGroupSet.Clear();
            GenStep_Cavern.tmpGroupSet.AddRange(group);
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].DistanceToEdge(map) >= 3 && caves[group[i]] <= 0f)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        IntVec3 item = group[i] + cardinalDirections[j];
                        if (!GenStep_Cavern.tmpGroupSet.Contains(item))
                        {
                            GenStep_Cavern.tmpCells.Add(group[i]);
                            break;
                        }
                    }
                }
            }
            if (!GenStep_Cavern.tmpCells.Any<IntVec3>())
            {
                //Log.Warning("Could not find any valid edge cell.");
                return group.RandomElement<IntVec3>();
            }
            return GenStep_Cavern.tmpCells.RandomElement<IntVec3>();
        }

        private float FindBestInitialDir(IntVec3 start, List<IntVec3> group, out float dist)
        {
            float num = (float)this.GetDistToNonRock(start, group, IntVec3.East, 40);
            float num2 = (float)this.GetDistToNonRock(start, group, IntVec3.West, 40);
            float num3 = (float)this.GetDistToNonRock(start, group, IntVec3.South, 40);
            float num4 = (float)this.GetDistToNonRock(start, group, IntVec3.North, 40);
            float num5 = (float)this.GetDistToNonRock(start, group, IntVec3.NorthWest, 40);
            float num6 = (float)this.GetDistToNonRock(start, group, IntVec3.NorthEast, 40);
            float num7 = (float)this.GetDistToNonRock(start, group, IntVec3.SouthWest, 40);
            float num8 = (float)this.GetDistToNonRock(start, group, IntVec3.SouthEast, 40);
            dist = Mathf.Max(new float[]
            {
                num,
                num2,
                num3,
                num4,
                num5,
                num6,
                num7,
                num8
            });
            return GenMath.MaxByRandomIfEqual<float>(0f, num + num8 / 2f + num6 / 2f, 45f, num8 + num3 / 2f + num / 2f, 90f, num3 + num8 / 2f + num7 / 2f, 135f, num7 + num3 / 2f + num2 / 2f, 180f, num2 + num7 / 2f + num5 / 2f, 225f, num5 + num4 / 2f + num2 / 2f, 270f, num4 + num6 / 2f + num5 / 2f, 315f, num6 + num4 / 2f + num / 2f);
        }

        private void Dig(IntVec3 start, float dir, float width, List<IntVec3> group, Map map, bool closed, HashSet<IntVec3> visited = null)
        {
            Vector3 vector = start.ToVector3Shifted();
            IntVec3 intVec = start;
            float num = 0f;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid caves = MapGenerator.Caves;
            bool flag = false;
            bool flag2 = false;
            if (visited == null)
            {
                visited = new HashSet<IntVec3>();
            }
            GenStep_Cavern.tmpGroupSet.Clear();
            GenStep_Cavern.tmpGroupSet.AddRange(group);
            int num2 = 0;
            while (true)
            {
                if (closed)
                {
                    int num3 = GenRadial.NumCellsInRadius(width / 2f + 1.5f);
                    for (int i = 0; i < num3; i++)
                    {
                        IntVec3 intVec2 = intVec + GenRadial.RadialPattern[i];
                        if (!visited.Contains(intVec2))
                        {
                            if (!GenStep_Cavern.tmpGroupSet.Contains(intVec2) || caves[intVec2] > 0f)
                            {
                                return;
                            }
                        }
                    }
                }
                if (num2 >= 30 && width > 4f + GenStep_Cavern.BranchedTunnelWidthOffset.max)
                {
                    if (!flag && Rand.Chance(0.6f))
                    {
                        this.DigInBestDirection(intVec, dir, new FloatRange(40f, 90f), width - GenStep_Cavern.BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
                        flag = true;
                    }
                    if (!flag2 && Rand.Chance(0.6f))
                    {
                        this.DigInBestDirection(intVec, dir, new FloatRange(-90f, -40f), width - GenStep_Cavern.BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
                        flag2 = true;
                    }
                }
                bool flag3;
                this.SetCaveAround(intVec, width, map, visited, out flag3);
                if (flag3)
                {
                    return;
                }
                while (vector.ToIntVec3() == intVec)
                {
                    vector += Vector3Utility.FromAngleFlat(dir) * 0.5f;
                    num += 0.5f;
                }
                if (!GenStep_Cavern.tmpGroupSet.Contains(vector.ToIntVec3()))
                {
                    return;
                }
                IntVec3 intVec3 = new IntVec3(intVec.x, 0, vector.ToIntVec3().z);
                if (this.IsRock(intVec3, elevation, map))
                {
                    caves[intVec3] = Mathf.Max(caves[intVec3], width);
                    visited.Add(intVec3);
                }
                intVec = vector.ToIntVec3();
                dir += (float)this.directionNoise.GetValue((double)(num * 60f), (double)((float)start.x * 200f), (double)((float)start.z * 200f)) * 8f;
                width -= 0.01f;
                if (width < 1.4f)
                {
                    return;
                }
                num2++;
            }
        }

        private void DigInBestDirection(IntVec3 curIntVec, float curDir, FloatRange dirOffset, float width, List<IntVec3> group, Map map, bool closed, HashSet<IntVec3> visited = null)
        {
            int num = -1;
            float dir = -1f;
            for (int i = 0; i < 6; i++)
            {
                float num2 = curDir + dirOffset.RandomInRange;
                int distToNonRock = this.GetDistToNonRock(curIntVec, group, num2, 50);
                if (distToNonRock > num)
                {
                    num = distToNonRock;
                    dir = num2;
                }
            }
            if (num < 18)
            {
                return;
            }
            this.Dig(curIntVec, dir, width, group, map, closed, visited);
        }

        private void SetCaveAround(IntVec3 around, float tunnelWidth, Map map, HashSet<IntVec3> visited, out bool hitAnotherTunnel)
        {
            hitAnotherTunnel = false;
            int num = GenRadial.NumCellsInRadius(tunnelWidth / 2f);
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid caves = MapGenerator.Caves;
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = around + GenRadial.RadialPattern[i];
                if (this.IsRock(intVec, elevation, map))
                {
                    if (caves[intVec] > 0f && !visited.Contains(intVec))
                    {
                        hitAnotherTunnel = true;
                    }
                    caves[intVec] = Mathf.Max(caves[intVec], tunnelWidth);
                    visited.Add(intVec);
                }
            }
        }

        private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, IntVec3 offset, int maxDist)
        {
            GenStep_Cavern.groupSet.Clear();
            GenStep_Cavern.groupSet.AddRange(group);
            for (int i = 0; i <= maxDist; i++)
            {
                IntVec3 item = from + offset * i;
                if (!GenStep_Cavern.groupSet.Contains(item))
                {
                    return i;
                }
            }
            return maxDist;
        }

        private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, float dir, int maxDist)
        {
            GenStep_Cavern.groupSet.Clear();
            GenStep_Cavern.groupSet.AddRange(group);
            Vector3 vector = Vector3Utility.FromAngleFlat(dir);
            for (int i = 0; i <= maxDist; i++)
            {
                IntVec3 item = (from.ToVector3Shifted() + vector * (float)i).ToIntVec3();
                if (!GenStep_Cavern.groupSet.Contains(item))
                {
                    return i;
                }
            }
            return maxDist;
        }

        private float GetDistToCave(IntVec3 cell, List<IntVec3> group, Map map, float maxDist, bool treatOpenSpaceAsCave)
        {
            MapGenFloatGrid caves = MapGenerator.Caves;
            GenStep_Cavern.tmpGroupSet.Clear();
            GenStep_Cavern.tmpGroupSet.AddRange(group);
            int num = GenRadial.NumCellsInRadius(maxDist);
            IntVec3[] radialPattern = GenRadial.RadialPattern;
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = cell + radialPattern[i];
                if ((treatOpenSpaceAsCave && !GenStep_Cavern.tmpGroupSet.Contains(intVec)) || (intVec.InBounds(map) && caves[intVec] > 0f))
                {
                    return cell.DistanceTo(intVec);
                }
            }
            return maxDist;
        }

        private void RemoveSmallDisconnectedSubGroups(List<IntVec3> group, Map map)
        {
            GenStep_Cavern.groupSet.Clear();
            GenStep_Cavern.groupSet.AddRange(group);
            GenStep_Cavern.groupVisited.Clear();
            for (int i = 0; i < group.Count; i++)
            {
                if (!GenStep_Cavern.groupVisited.Contains(group[i]) && GenStep_Cavern.groupSet.Contains(group[i]))
                {
                    GenStep_Cavern.subGroup.Clear();
                    map.floodFiller.FloodFill(group[i], (IntVec3 x) => GenStep_Cavern.groupSet.Contains(x), delegate(IntVec3 x)
                    {
                        GenStep_Cavern.subGroup.Add(x);
                        GenStep_Cavern.groupVisited.Add(x);
                    }, 2147483647, false, null);
                    if (GenStep_Cavern.subGroup.Count < 100 || (float)GenStep_Cavern.subGroup.Count < 0.05f * (float)group.Count)
                    {
                        for (int j = 0; j < GenStep_Cavern.subGroup.Count; j++)
                        {
                            GenStep_Cavern.groupSet.Remove(GenStep_Cavern.subGroup[j]);
                        }
                    }
                }
            }
            group.Clear();
            group.AddRange(GenStep_Cavern.groupSet);
        }
    }

}
