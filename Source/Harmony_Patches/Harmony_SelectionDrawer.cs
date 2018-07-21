using Harmony;
using RimWorld;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(SelectionDrawer))]
    [HarmonyPatch("DrawSelectionOverlays")]
    public class Harmony_SelectionDrawer_DrawSelectionOverlays
    {

        public static bool Prefix()
        {
            if (Find.CurrentMap.GetComponent<MapComponent_RenderManager>().Rendering)
            {
                return false;
            }
            return true;
        }

    }

}
