using RimWorld;
using Verse;

namespace ProgressRenderer
{
	
	[DefOf]
	public static class DesignationDefOf
	{
		
		static DesignationDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DesignationDefOf));
		}

		public static DesignationDef CornerMarker;

	}

}
