using Harmony;
using RimWorld;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(EnvironmentStatsDrawer))]
    [HarmonyPatch("DrawRoomOverlays")]
    public class Harmony_EnvironmentStatsDrawer_DrawRoomOverlays
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
