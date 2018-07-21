using Harmony;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(RoofGrid))]
    [HarmonyPatch("RoofGridUpdate")]
    public class Harmony_RoofGrid_RoofGridUpdate
    {

        public static bool Prefix(Map ___map)
        {
            if (___map.GetComponent<MapComponent_RenderManager>().Rendering)
            {
                return false;
            }
            return true;
        }

    }

}
