using System;
using RimWorld;
using Verse;
using System.Xml;
namespace RimWorldBiomesCave
{
    public class BiomesCaveSettings : ModSettings
    {
        public bool enableAnimations = true;
        public bool enableAnimalLight = true;
        public bool enablePlantOverlay = false;
        public bool enableWaterPathing = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.enableWaterPathing, "enableWaterPathing", true);
            Scribe_Values.Look(ref this.enableAnimations, "enableAnimations", true);
            Scribe_Values.Look(ref this.enableAnimalLight, "enableAnimalLight", true);
            Scribe_Values.Look(ref this.enablePlantOverlay, "enablePlantOverlay", true);
        }
    }

    public class CheckWaterPathingSetting : PatchOperation{
        protected override bool ApplyWorker(XmlDocument xml)
        {
            //Log.Error(BiomesCavesMod.settings.enableAnimalLight.ToString());
            return BiomesCaveMod.settings.enableWaterPathing;
        }
    }

    public class CheckLightSetting : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            //Log.Error(BiomesCavesMod.settings.enableAnimalLight.ToString());
            return BiomesCaveMod.settings.enableAnimalLight;
        }
    }

    public class CheckAnimationSetting : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            //Log.Error(BiomesCavesMod.settings.enableAnimalLight.ToString());
            return BiomesCaveMod.settings.enableAnimations;
        }
    }

    public class CheckPlantOverlaySetting : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            //Log.Error(BiomesCavesMod.settings.enableAnimalLight.ToString());
            return BiomesCaveMod.settings.enablePlantOverlay;
        }
    }

}
