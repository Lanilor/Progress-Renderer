using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ProgressRenderer
{
    
    [HarmonyPatch(typeof(OverlayDrawer))]
    [HarmonyPatch("DrawAllOverlays")]
    public class Harmony_OverlayDrawer_DrawAllOverlays
    {

        public static bool Prefix()
        {
            if (!PRModSettings.renderThingIcons && Find.CurrentMap.GetComponent<MapComponent_RenderManager>().Rendering)
            {
                return false;
            }
            return true;
        }

    }
    
}
