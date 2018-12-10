using RimWorld;
using Verse;

namespace ProgressRenderer
{
	
	[DefOf]
	public static class KeyBindingDefOf
	{

        static KeyBindingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingDefOf));
		}

        public static KeyBindingDef LPR_ManualRendering;
        public static KeyBindingDef LPR_ManualRendering_ForceFullMap;

	}

}
