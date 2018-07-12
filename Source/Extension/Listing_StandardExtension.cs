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

        private const float BaseLineHeight = 30f;

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
            Rect rectWrapper = ls.GetRect(BaseLineHeight);
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

        public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, float min, float max, string format = null, string tooltip = null)
        {
            // Backup original values
            TextAnchor backupAnchor = Text.Anchor;
            // Wrapper
            Rect rectWrapper = ls.GetRect(BaseLineHeight);
            Rect rectLeft = rectWrapper.LeftHalf();
            Rect rectRight = rectWrapper.RightHalf();
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rectLeft, tooltip);
            }
            // Left
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLeft, label);
            // Right
            string sliderLabel = (format != null) ? val.ToString(format) : val.ToString();
            val = Widgets.HorizontalSlider(rectRight, val, min, max, true, sliderLabel);
            // Restore original values
            Text.Anchor = backupAnchor;
        }
        public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, int min, int max, string format = null, string tooltip = null)
        {
            float valFloat = val;
            ls.SliderLabeled(label, ref valFloat, min, max, format, tooltip);
            val = (int)valFloat;
        }

        public static void FrequencySliderLabeled(this Listing_Standard ls, string label, ref float val, float min, float max, string tooltip = null)
        {
            // Backup original values
            TextAnchor backupAnchor = Text.Anchor;
            // Wrapper
            Rect rectWrapper = ls.GetRect(BaseLineHeight);
            Rect rectLeft = rectWrapper.LeftHalf();
            Rect rectRight = rectWrapper.RightHalf();
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rectLeft, tooltip);
            }
            // Left
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLeft, label);
            // Right
            val = Widgets.FrequencyHorizontalSlider(rectRight, val, min, max, true);
            // Restore original values
            Text.Anchor = backupAnchor;
        }

        public static void FixedFrequencySliderLabeled(this Listing_Standard ls, string label, ref int val, string tooltip = null)
        {
            // Backup original values
            TextAnchor backupAnchor = Text.Anchor;
            // Wrapper
            Rect rectWrapper = ls.GetRect(BaseLineHeight);
            Rect rectLeft = rectWrapper.LeftHalf();
            Rect rectRight = rectWrapper.RightHalf();
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rectLeft, tooltip);
            }
            // Left
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLeft, label);
            // Right
            val = MoreWidgets.FixedFrequencyHorizontalSlider(rectRight, val);
            // Restore original values
            Text.Anchor = backupAnchor;
        }

    }

}
