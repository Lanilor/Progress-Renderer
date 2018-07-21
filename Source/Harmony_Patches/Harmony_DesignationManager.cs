using Harmony;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(DesignationManager))]
    [HarmonyPatch("DrawDesignations")]
    public class Harmony_DesignationManager_DrawDesignations
    {

        public static bool Prefix(DesignationManager __instance)
        {
            if (!PRModSettings.renderDesignations && __instance.map.GetComponent<MapComponent_RenderManager>().Rendering)
            {
                return false;
            }
            return true;
        }

    }
    
}
