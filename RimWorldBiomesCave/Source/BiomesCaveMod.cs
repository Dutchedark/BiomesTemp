using System;
using RimWorld;
using Verse;
namespace RimWorldBiomesCave
{
    public class BiomesCaveMod : Mod
    {
        public static BiomesCaveSettings settings;

        public BiomesCaveMod(ModContentPack content) : base(content){
            settings = GetSettings<BiomesCaveSettings>();
        }

        public override string SettingsCategory() => "BiomesCaveSettingsCategoryLabel".Translate();

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("EnableWaterPathingLabel".Translate() + ": ", ref settings.enableWaterPathing);
            listing_Standard.CheckboxLabeled("EnableAnimationsLabel".Translate() + ": ", ref settings.enableAnimations);
            listing_Standard.CheckboxLabeled("EnableAnimalLightLabel".Translate() + ": ", ref settings.enableAnimalLight);
            listing_Standard.CheckboxLabeled("EnablePlantOverlayLabel".Translate() + ": ", ref settings.enablePlantOverlay);
            listing_Standard.End();
            settings.Write();
        }
    }
}
