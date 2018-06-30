using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using UnityEngine;
using System.Reflection.Emit;

namespace ProgressRenderer
{
    /*
    [HarmonyPatch(typeof(MainMenuDrawer))]
    [HarmonyPatch("DoMainMenuControls")]
    public class Harmony_MainMenuDrawer_DoMainMenuControls
    {

        private static void DoMainMenuHelper()
        {
            
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            for (int i = 0, iLen = instructions.Count(); i < iLen; i++)
            {
                CodeInstruction ci = instructions.ElementAt(i);
                if (ci.opcode == OpCodes.Ldarg_1)
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_MainMenuDrawer_DoMainMenuControls), "DoMainMenuHelper"));
                }
                yield return ci;
            }
            yield break;
        }

    }
    */
}
