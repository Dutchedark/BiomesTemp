using Harmony;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System;

namespace RimWorldBiomesCave
{
	[StaticConstructorOnStartup]
    public class CompProperties_ReactiveDefense : CompProperties
    {
        public enum DefTrigger{
            health,
            proximity,
            attacked
        }
        public enum DefType
        {
            projectile,
            aura,
            hide,
            buff,
            trail,
            reflect
        }
        public float hpThreshold = 0.2f;
        public int proximity = 2;
        public int duration = 3;
        public int auraSize = 1;
        public String aura;
        public DefTrigger defenseTrigger;
        public DefType defenseType;
        public float reflectPercent = 0.2f;
        public GraphicData hideGraphic = null;
        public CompProperties_ReactiveDefense()
        {
            this.compClass = typeof(CompReactiveDefense);
        }
        public ThingDef apparel = null;
        public float moveSpeedPenalty = 0.5f;
        public bool stopAttacker = true;
        public List<StatDef> statDefs;
        public List<float> statValues;
    }
}
