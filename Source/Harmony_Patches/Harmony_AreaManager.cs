using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(AreaManager))]
    [HarmonyPatch("AreaManagerUpdate")]
    public class Harmony_AreaManager_AreaManagerUpdate
    {

        public static bool Prefix(AreaManager __instance)
        {
            if (__instance.map.GetComponent<MapComponent_RenderManager>().Rendering)
            {
                return false;
            }
            return true;
        }

    }
    
}
