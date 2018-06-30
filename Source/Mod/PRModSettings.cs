using System;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace ProgressRenderer
{

    public class PRModSettings : ModSettings
    {

        public static string[] SupportedImageFormats = { "JPG", "PNG" };

        private static bool DefaultEnabled = true;
        private static bool DefaultRenderWeather = false;
        private static int DefaultInterval = 1;
        private static int DefaultTimeOfDay = 8;
        private static string DefaultImageFormat = "JPG";
        private static int DefaultPixelPerCell = 20;

        public static bool enabled = DefaultEnabled;
        public static bool renderWeather = DefaultRenderWeather;
        public static int interval = DefaultInterval;
        public static int timeOfDay = DefaultTimeOfDay;
        public static string imageFormat = DefaultImageFormat;
        public static int pixelPerCell = DefaultPixelPerCell;
        public static string exportPath;

        public PRModSettings() : base()
        {
            if (exportPath.NullOrEmpty())
            {
                exportPath = DesktopPath;
            }
        }

        public void DoWindowContents(Rect rect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);
            ls.Gap();

            ls.CheckboxLabeled("LPR_SettingsEnabledLabel".Translate(), ref enabled, "LPR_SettingsEnabledDescription".Translate());
            ls.GapGapLine();

            ls.CheckboxLabeled("LPR_SettingsRenderWeatherLabel".Translate(), ref renderWeather, "LPR_SettingsRenderWeatherDescription".Translate());
            ls.GapGapLine();

            ls.SliderLabeled("LPR_SettingsIntervalLabel".Translate(), ref interval, 1, 60, "LPR_SettingsIntervalDescription".Translate());
            ls.Gap();
            ls.SliderLabeled("LPR_SettingsTimeOfDayLabel".Translate(), ref timeOfDay, 0, 23, "LPR_SettingsTimeOfDayDescription".Translate());
            ls.GapGapLine();

            if (ls.ButtonTextLabeled("LPR_SettingsImageFormatLabel".Translate(), imageFormat))
            {
                List<FloatMenuOption> menuEntries = new List<FloatMenuOption>();
                menuEntries.Add(new FloatMenuOption(SupportedImageFormats[0], delegate
                {
                    imageFormat = SupportedImageFormats[0];
                }));
                menuEntries.Add(new FloatMenuOption(SupportedImageFormats[1], delegate
                {
                    imageFormat = SupportedImageFormats[1];
                }));
                Find.WindowStack.Add(new FloatMenu(menuEntries));
            }
            ls.Gap();
            ls.SliderLabeled("LPR_SettingsPixelPerCellLabel".Translate(), ref pixelPerCell, 1, 50, "LPR_SettingsPixelPerCellDescription".Translate());
            ls.Gap();
            ls.TextFieldLabeled("LPR_SettingsExportPathLabel".Translate(), ref exportPath, "LPR_SettingsExportPathDescription".Translate());

            ls.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enabled, "enabled", DefaultEnabled);
            Scribe_Values.Look(ref renderWeather, "renderWeather", DefaultRenderWeather);
            Scribe_Values.Look(ref interval, "interval", DefaultInterval);
            Scribe_Values.Look(ref timeOfDay, "timeOfDay", DefaultTimeOfDay);
            Scribe_Values.Look(ref imageFormat, "imageFormat", DefaultImageFormat);
            Scribe_Values.Look(ref pixelPerCell, "pixelPerCell", DefaultPixelPerCell);
            Scribe_Values.Look(ref exportPath, "exportPath", DesktopPath);
        }

        private static string DesktopPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

    }

}
