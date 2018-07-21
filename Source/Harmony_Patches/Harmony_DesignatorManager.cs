using Harmony;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(DesignatorManager))]
    [HarmonyPatch("DesignatorManagerUpdate")]
    public class Harmony_DesignatorManager_DesignatorManagerUpdate
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
