using Harmony;
using RimWorld;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(Targeter))]
    [HarmonyPatch("TargeterUpdate")]
    public class Harmony_Targeter_TargeterUpdate
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
