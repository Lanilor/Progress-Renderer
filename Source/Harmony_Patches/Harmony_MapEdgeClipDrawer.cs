using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(MapEdgeClipDrawer))]
    [HarmonyPatch("DrawClippers")]
    public class Harmony_MapEdgeClipDrawer_DrawClippers
    {

        public static bool Prefix(Map map)
        {
            if (map.GetComponent<MapComponent_RenderManager>().Rendering)
            {
                return false;
            }
            return true;
        }

    }
    
}
