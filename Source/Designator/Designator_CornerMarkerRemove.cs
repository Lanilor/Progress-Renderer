using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace ProgressRenderer
{

	public class Designator_CornerMarkerRemove : Designator_CornerMarker
	{

        public Designator_CornerMarkerRemove() : base(DesignateMode.Remove)
		{
			this.defaultLabel = "DesignatorConerMarkerRemove".Translate();
			this.defaultDesc = "DesignatorCornerMarkerRemoveDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/CornerMarkerOff");
			this.soundSucceeded = SoundDefOf.Designate_PlanRemove;
		}

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

	}

}
