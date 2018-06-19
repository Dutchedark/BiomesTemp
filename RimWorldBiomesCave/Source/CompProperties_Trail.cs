﻿using System;
using Verse;
namespace RimWorldBiomesCave
{
    [StaticConstructorOnStartup]
    public class CompProperties_Trail : CompProperties
    {
        public ThingDef trail = null;
        public int maxStacks = 4;
        public CompProperties_Trail()
        {
            this.compClass = typeof(Comp_Trail);
        }
    }
}
