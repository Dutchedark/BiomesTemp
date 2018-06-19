using Harmony;
using RimWorld;
using Verse;
using System;

namespace RimWorldBiomesCave
{
	[StaticConstructorOnStartup]
    public class CompProperties_AuraParticle : CompProperties
    {
        public enum Parent{
            animal,
            building,
            item,
            plant
        }
        public String parentThing;
        public Parent releasedBy = Parent.item;
        public int duration = 3;
        public String hediff;
        public int damage;
        public DamageDef damageType;
        public float severity = 1f;
        public bool forceFlee = false;
        public bool warn = false;
        public CompProperties_AuraParticle()
        {
			this.compClass = typeof(CompAuraParticle);
        }
    }
}
