using Harmony;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(DeepResourceGrid))]
    [HarmonyPatch("DeepResourceGridUpdate")]
    public class Harmony_DeepResourceGrid_DeepResourceGridUpdate
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
