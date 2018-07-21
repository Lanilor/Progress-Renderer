using UnityEngine;
using Verse;

namespace ProgressRenderer
{

    public class PRMod : Mod
    {

        public PRModSettings settings;

        public PRMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<PRModSettings>();
        }

        public override string SettingsCategory() {
            return "LPR_SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            settings.DoWindowContents(rect);
            settings.Write();
        }

    }

}
