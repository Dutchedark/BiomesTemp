using Verse;

namespace RimWorldBiomesCave
{
    [StaticConstructorOnStartup]
    public class CompProperties_IgnoreDeepWater : CompProperties
    {
		public CompProperties_IgnoreDeepWater()
		{
			this.compClass = typeof(CompIgnoreDeepWater);
		}


    }
}
