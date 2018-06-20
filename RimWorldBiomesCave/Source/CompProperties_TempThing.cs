using System;
using UnityEngine;
using Verse;
namespace RimWorldBiomesCave
{
    public class CompProperties_TempThing : CompProperties
    {
        public float lifetime = 5;
        public CompProperties_TempThing()
        {
            this.compClass = typeof(Comp_TempThing);
        }
    }
}
