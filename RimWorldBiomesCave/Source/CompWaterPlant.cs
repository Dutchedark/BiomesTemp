using Harmony;
using RimWorld;
using Verse;

namespace RimWorldBiomesCave
{
    public class CompWaterPlant : ThingComp
    {
        public CompProperties_WaterPlant Props{
            get{
                return (CompProperties_WaterPlant)this.props;
            }
        }



    }
}
