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
using Harmony;

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
            // Update lastRenderedDay
            lastRenderedDay = currDay;
            // Start rendering if it is enabled
            if (PRModSettings.enabled)
            {
                //LongEventHandler.QueueLongEvent(new Action(StartRendering), "LPR_Rendering", false, null);
                Find.CameraDriver.StartCoroutine(DoRendering());
            }
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

            // Calculate basic values that are used for rendering
            int imageWidth = map.Size.x * PRModSettings.pixelPerCell;
            int imageHeight = map.Size.z * PRModSettings.pixelPerCell;

            int renderCountX = (int)Math.Ceiling((float)imageWidth / 4096);
            int renderCountZ = (int)Math.Ceiling((float)imageHeight / 4096);
            int renderWidth = (int)Math.Ceiling((float)imageWidth / renderCountX);
            int renderHeight = (int)Math.Ceiling((float)imageHeight / renderCountZ);

            float cameraPosX = (float)map.Size.x / 2 / renderCountX;
            float cameraPosZ = (float)map.Size.z / 2 / renderCountZ;
            float orthographicSize = Math.Min(cameraPosX, cameraPosZ);
            Vector3 cameraPos = new Vector3(cameraPosX, 15f + (orthographicSize - 11f) / 49f * 50f, cameraPosZ);

            RenderTexture renderTexture = new RenderTexture(renderWidth, renderHeight, 24);
            Texture2D imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

            Camera camera = Find.Camera;
            CameraDriver camDriver = camera.GetComponent<CameraDriver>();
            camDriver.enabled = false;

            // Store current camera data
            Vector3 rememberedRootPos = map.rememberedCameraPos.rootPos;
            float rememberedRootSize = map.rememberedCameraPos.rootSize;
            float rememberedFarClipPlane = camera.farClipPlane;

            // Overwrite current view rect in the camera driver
            Traverse.Create(camDriver).Field("lastViewRect").SetValue(new CellRect(0, 0, map.Size.x, map.Size.z));
            yield return new WaitForEndOfFrame();

            // Set camera values needed for rendering
            camera.orthographicSize = orthographicSize;
            camera.farClipPlane = cameraPos.y + 6.5f;
            
            // Create a temporary camera for rendering
            /*Camera tmpCam = Camera.Instantiate(camera);
            tmpCam.orthographicSize = orthographicSize;
            tmpCam.farClipPlane = cameraPos.y + 6.5f;*/

            // Set render textures
            //tmpCam.targetTexture = renderTexture;
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;

            // Render the image texture
            if (PRModSettings.renderWeather)
            {
                map.weatherManager.DrawAllWeather();
            }
            for (int i = 0; i < renderCountZ; i++)
            {
                for (int j = 0; j < renderCountX; j++)
                {
                    camera.transform.position = new Vector3(cameraPos.x * (2 * j + 1), cameraPos.y, cameraPos.z * (2 * i + 1));
                    camera.Render();
                    imageTexture.ReadPixels(new Rect(0, 0, renderWidth, renderHeight), renderWidth * j, renderHeight * i, false);
                }
            }

            // Encode and safe image to file
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
            //tmpCam.targetTexture = null;
            camera.targetTexture = null;
            camera.farClipPlane = rememberedFarClipPlane;
            camDriver.SetRootPosAndSize(rememberedRootPos, rememberedRootSize);
            camDriver.enabled = true;

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
            yield break;
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
