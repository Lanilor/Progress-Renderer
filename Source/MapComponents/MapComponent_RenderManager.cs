using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections;
using RimWorld.Planet;

namespace ProgressRenderer
{

    public class MapComponent_RenderManager : MapComponent
    {

        private int lastRenderedDay = -1;

        public MapComponent_RenderManager(Map map) : base(map)
        {

        }

        /*
        public override void FinalizeInit()
        {
            // New map and after loading
        }

        public override void MapGenerated()
        {
            // Only new map
        }
        */

        public override void MapComponentTick()
        {
            // TickRare
            if (Find.TickManager.TicksGame % 250 != 0)
            {
                return;
            }
            // Only render player home maps
            if (!map.IsPlayerHome)
            {
                return;
            }
            // Check for correct day
            int currDay = GenDate.DaysPassed;
            if (currDay <= lastRenderedDay || currDay % PRModSettings.interval != 0)
            {
                return;
            }
            // Check for correct hour
            Vector2 longLat = Find.WorldGrid.LongLatOf(map.Tile);
            int hour = GenDate.HourInteger(Find.TickManager.TicksAbs, longLat.x);
            if (hour != PRModSettings.timeOfDay)
            {
                return;
            }
            // Start rendering
            lastRenderedDay = currDay;
            Find.CameraDriver.StartCoroutine(DoRendering());
            LongEventHandler.QueueLongEvent(new Action(StartRendering), "LPR_Rendering", false, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref lastRenderedDay, "lastRenderedDay", -1);
        }

        private void StartRendering()
        {
            // TODO
        }

        private IEnumerator DoRendering()
        {
            // Temporary switch to this map for rendering
            bool switchedMap = false;
            Map rememberedMap = Find.CurrentMap;
            if (map != rememberedMap)
            {
                switchedMap = true;
                Current.Game.CurrentMap = map;
            }

            // Close world view if needed
            bool rememberedWorldRendered = WorldRendererUtility.WorldRenderedNow;
            if (rememberedWorldRendered)
            {
                CameraJumper.TryHideWorld();
            }

            // Calculate camera positions
            float cameraPosX = (float)map.Size.x / 2;
            float cameraPosZ = (float)map.Size.z / 2;
            float cameraWidth = (float)map.Size.x;
            float cameraHeight = (float)map.Size.z;
            float orthographicSize = Math.Min(cameraWidth, cameraHeight) / 2;
            
            int imageWidth = (int)cameraWidth * PRModSettings.pixelPerCell;
            int imageHeight = (int)cameraHeight * PRModSettings.pixelPerCell;

            RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 32, RenderTextureFormat.ARGB32);
            Texture2D imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

            Camera camera = Find.Camera;
            CameraDriver camDriver = camera.GetComponent<CameraDriver>();

            // Store current camera data
            Vector3 rememberedRootPos = map.rememberedCameraPos.rootPos;
            float rememberedRootSize = map.rememberedCameraPos.rootSize;
            float rememberedFarClipPlane = camera.farClipPlane;

            camDriver.enabled = false;

            Vector3 cameraPos = new Vector3(cameraPosX, rememberedRootPos.y, cameraPosZ);

            // Set values in the first frame to refresh the cache later
            camDriver.SetRootPosAndSize(cameraPos, orthographicSize);
            // Extend farClipPlane to new distance
            camera.farClipPlane = camera.transform.position.y + 6.5f;
            // Reset values directly on the camera to avoid display flashing
            camera.orthographicSize = rememberedRootSize;
            camera.transform.position = rememberedRootPos;
            yield return new WaitForEndOfFrame();

            // Set values again for the second frame (the final rendering frame)
            camDriver.SetRootPosAndSize(cameraPos, orthographicSize);
            yield return new WaitForEndOfFrame();

            camera.targetTexture = rt;
            RenderTexture.active = rt;

            // Render the image texture
            if (PRModSettings.renderWeather)
            {
                map.weatherManager.DrawAllWeather();
            }
            camera.Render();

            // Safe image to file
            imageTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0, false);
            imageTexture.Apply();
            byte[] encodedImage;
            if (PRModSettings.imageFormat == "PNG")
            {
                encodedImage = imageTexture.EncodeToPNG();
            }
            else
            {
                encodedImage = imageTexture.EncodeToJPG();
            }
            File.WriteAllBytes(CreateCurrentFilePath(), encodedImage);

            // Restore camera and viewport
            RenderTexture.active = null;
            camera.targetTexture = null;
            camera.farClipPlane = rememberedFarClipPlane;
            camDriver.SetRootPosAndSize(rememberedRootPos, rememberedRootSize);
            camera.GetComponent<CameraDriver>().enabled = true;

            // Switch back to world view if needed
            if (rememberedWorldRendered)
            {
                CameraJumper.TryShowWorld();
            }

            // Switch back to remembered map if needed
            if (switchedMap)
            {
                Current.Game.CurrentMap = rememberedMap;
            }
        }

        private string CreateCurrentFilePath()
        {
            string imageName = "rimworld-" + Find.World.info.seedString + "-" + map.Tile + "-" + GenDate.DaysPassed;
            string fileExt = PRModSettings.imageFormat.ToLower();
            string filePath = Path.Combine(PRModSettings.exportPath, imageName + "." + fileExt);
            if (!File.Exists(filePath))
            {
                return filePath;
            }
            int i = 1;
            filePath = Path.Combine(PRModSettings.exportPath, imageName);
            string newPath;
            do
            {
                newPath = filePath + "-" + i + "." + fileExt;
                i++;
            }
            while (File.Exists(newPath));
            return newPath;
        }

    }

}
