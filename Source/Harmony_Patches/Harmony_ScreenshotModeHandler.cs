using Harmony;
using UnityEngine;
using Verse;

namespace ProgressRenderer
{

    [HarmonyPatch(typeof(ScreenshotModeHandler))]
    [HarmonyPatch("ScreenshotModesOnGUI")]
    public class Harmony_ScreenshotModeHandler_ScreenshotModesOnGUI
    {

        public static void Postfix()
        {
            if (KeyBindingDefOf.LPR_ManualRendering.KeyDownEvent)
            {
                MapComponent_RenderManager.TriggerCurrentMapManualRendering();
                Event.current.Use();
            }
            else if (KeyBindingDefOf.LPR_ManualRendering_ForceFullMap.KeyDownEvent)
            {
                MapComponent_RenderManager.TriggerCurrentMapManualRendering(true);
                Event.current.Use();
            }
        }

    }
    
}
