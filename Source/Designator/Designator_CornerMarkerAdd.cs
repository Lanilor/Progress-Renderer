using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressRenderer
{

	public class Designator_CornerMarkerAdd : Designator_CornerMarker
	{

        public Designator_CornerMarkerAdd() : base(DesignateMode.Add)
		{
			this.defaultLabel = "DesignatorCornerMarker".Translate();
			this.defaultDesc = "DesignatorCornerMarkerDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/CornerMarkerOn");
			this.soundSucceeded = SoundDefOf.Designate_PlanAdd;
		}

	}

}
