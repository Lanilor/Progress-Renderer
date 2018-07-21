using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(GameConditionManager))]
    [HarmonyPatch("GameConditionManagerDraw")]
    public class Harmony_GameConditionManager_GameConditionManagerDraw
    {

        public static bool Prefix(Map map)
        {
            if (!PRModSettings.renderGameConditions && map.GetComponent<MapComponent_RenderManager>().Rendering)
            {
                return false;
            }
            return true;
        }

    }
    
}
