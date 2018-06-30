using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace ProgressRenderer
{

    public static class Listing_StandardExtension
    {

        public static void GapGapLine(this Listing_Standard ls)
        {
            ls.Gap();
            ls.GapLine();
            ls.Gap();
        }

        public static void TextFieldLabeled(this Listing_Standard ls, string label, ref string val, string tooltip = null)
        {
            // Backup original values
            TextAnchor backupAnchor = Text.Anchor;
            // Wrapper
            Rect rectWrapper = ls.GetRect(Text.LineHeight);
            Rect rectLeft = rectWrapper.LeftPart(0.5f).Rounded();
            Rect rectRight = rectWrapper.RightPart(0.5f).Rounded();
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rectLeft, tooltip);
            }
            // Left
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLeft, label);
            // Right
            val = Widgets.TextField(rectRight, val);
            // Restore original values
            Text.Anchor = backupAnchor;
        }

        public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, float min, float max, string tooltip = null)
        {
            // Backup original values
            TextAnchor backupAnchor = Text.Anchor;
            // Wrapper
            Rect rectWrapper = ls.GetRect(Text.LineHeight);
            Rect rectLeft = rectWrapper.LeftPart(0.5f).Rounded();
            Rect rectCenter = rectWrapper.RightPart(0.5f).LeftPart(0.8f).Rounded();
            Rect rectRight = rectWrapper.RightPart(0.1f).Rounded();
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rectLeft, tooltip);
                TooltipHandler.TipRegion(rectRight, tooltip);
            }
            // Left
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLeft, label);
            // Center
            val = Widgets.HorizontalSlider(rectCenter, val, min, max, true);
            // Right
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rectRight, val.ToString());
            // Restore original values
            Text.Anchor = backupAnchor;
        }
        public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, int min, int max, string tooltip = null)
        {
            float valFloat = val;
            ls.SliderLabeled(label, ref valFloat, min, max, tooltip);
            val = (int)valFloat;
        }

    }

}
