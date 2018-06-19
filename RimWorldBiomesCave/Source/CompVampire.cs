using Verse;
using Verse.AI;
using UnityEngine;
namespace RimWorldBiomesCave
{
    public class CompVampire : ThingComp
    {
        public CompProperties_Vampire Props
        {
            get
            {
                return (CompProperties_Vampire)this.props;
            }
        }
    }
}
