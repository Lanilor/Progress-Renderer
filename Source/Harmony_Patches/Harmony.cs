using System.Reflection;
using Harmony;
using Verse;

namespace ProgressRenderer
{

    [StaticConstructorOnStartup]
    class Harmony
    {

        static Harmony()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.lanilor.progressrenderer");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //harmony.Patch(AccessTools.Method(AccessTools.TypeByName("RiverMaker"), "ValidatePassage"), null, new HarmonyMethod(typeof(Harmony_RiverMaker_ValidatePassage), "Postfix"));
            
        }

    }

}
