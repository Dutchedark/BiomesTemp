using RimWorld;
using Verse;
using System;
namespace RimWorldBiomesCave
{
    public class CompBiomeSpecific : ThingComp
    {
        public CompProperties_BiomeSpecific Props
        {
            get{
                return (CompProperties_BiomeSpecific)this.props;
            }
        }
    }
}
