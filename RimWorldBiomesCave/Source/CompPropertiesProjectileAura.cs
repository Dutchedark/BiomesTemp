using System;
using Verse;
using RimWorld;
using System.Collections.Generic;
namespace RimWorldBiomesCave
{
    [StaticConstructorOnStartup]
    public class CompPropertiesProjectileAura : CompProperties
    {
        public int duration = 3;
        public String aura;
        public CompPropertiesProjectileAura()
        {
            this.compClass = typeof(CompProjectileAura);
        }
    }
}
