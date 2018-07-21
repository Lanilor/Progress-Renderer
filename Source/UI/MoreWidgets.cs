using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ProgressRenderer
{

    public static class MoreWidgets
    {

        private static List<int> FFHSPoints = new List<int>() { 15 * 24, 10 * 24, 6 * 24, 5 * 24, 4 * 24, 3 * 24, 2 * 24, 24, 12, 8, 6, 4, 3, 2, 1 };
        private static List<int> FFHSPointLabels = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 2, 2, 2, 3 };
        private static List<string> FFHSLabels = new List<string>() { "EveryDays", "EveryDay", "EveryHours", "EveryHour" };

        public static int FixedFrequencyHorizontalSlider(Rect rect, int freq)
        {
            int posInt = FFHSPoints.IndexOf(freq);
            if (posInt < 0) {
                Log.Error("Wrong configuration found for ProgressRenderer.PRModSettings.interval. Using default value.");
                posInt = FFHSPoints.IndexOf(24);
            }
            // Create label
            int labelIndex = FFHSPointLabels[posInt];
            float valForLabel = freq;
            if (labelIndex == 0)
            {
                valForLabel = valForLabel / 24f;
            }
            string label = FFHSLabels[labelIndex].Translate(new object[] { valForLabel.ToString("#0") });
            // Make slider
            float valInt = (float)posInt / (FFHSPoints.Count - 1);
            float valRet = Widgets.HorizontalSlider(rect, valInt, 0f, 1f, true, label);
            if (valInt != valRet)
            {
                int newPosInt = (int)Math.Round(valRet * (FFHSPoints.Count - 1));
                freq = FFHSPoints[newPosInt];
            }
            return freq;
        }

    }

}
