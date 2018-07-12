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

        private const int RenderTextureSize = 4096;
        private const int RenderTextureSizeSmall = 1024;

        private bool ctrlDoRendering = false;
        private bool ctrlCloseMessageBox = false;
        private int lastRenderedHour = -999;

        private SmallMessageBox messageBox;
        private Texture2D imageTexture;

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
            if (ctrlDoRendering)
            {
                // Start rendering if it is enabled
                if (PRModSettings.enabled)
                {
                    Find.CameraDriver.StartCoroutine(DoRendering());
                }
                ctrlDoRendering = false;
                ctrlCloseMessageBox = true;
                return;
            }
            if (ctrlCloseMessageBox)
            {
                if (messageBox != null)
                {
                    messageBox.Close();
                    messageBox = null;
                }
                ctrlCloseMessageBox = false;
                return;
            }

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
            // Check for correct time to render
            Vector2 longLat = Find.WorldGrid.LongLatOf(map.Tile);
            int currHour = MoreGenDate.HoursPassedInteger(Find.TickManager.TicksAbs, longLat.x);
            if (currHour <= lastRenderedHour)
            {
                return;
            }
            if (currHour % PRModSettings.interval != PRModSettings.timeOfDay % PRModSettings.interval)
            {
                return;
            }
            // Prepare for rendering
            lastRenderedHour = currHour;
            ctrlDoRendering = true;
            // Show message window
            if (PRModSettings.showMessageBox)
            {
                messageBox = new SmallMessageBox("LPR_Rendering".Translate());
                Find.WindowStack.Add(messageBox);
            }
            else
            {
                Messages.Message("LPR_Rendering".Translate(), MessageTypeDefOf.CautionInput, false);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref lastRenderedHour, "lastRenderedHour", -999);
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

            // Calculate rendered area
            int startX = 0;
            int startZ = 0;
            int endX = map.Size.x;
            int endZ = map.Size.z;
            List<Designation> cornerMarkers = map.designationManager.allDesignations.FindAll(des => des.def == DesignationDefOf.CornerMarker);
            if (cornerMarkers.Count > 1)
            {
                startX = endX;
                startZ = endZ;
                endX = 0;
                endZ = 0;
                foreach (Designation des in cornerMarkers)
                {
                    IntVec3 cell = des.target.Cell;
                    if (cell.x < startX) { startX = cell.x; }
                    if (cell.z < startZ) { startZ = cell.z; }
                    if (cell.x > endX) { endX = cell.x; }
                    if (cell.z > endZ) { endZ = cell.z; }
                }
                endX += 1;
                endZ += 1;
            }
            int distX = endX - startX;
            int distZ = endZ - startZ;
            
            // Calculate basic values that are used for rendering
            int imageWidth = distX * PRModSettings.pixelPerCell;
            int imageHeight = distZ * PRModSettings.pixelPerCell;

            int renderTextureSize = PRModSettings.saveMemory ? RenderTextureSizeSmall : RenderTextureSize;
            int renderCountX = (int)Math.Ceiling((float)imageWidth / renderTextureSize);
            int renderCountZ = (int)Math.Ceiling((float)imageHeight / renderTextureSize);
            int renderWidth = (int)Math.Ceiling((float)imageWidth / renderCountX);
            int renderHeight = (int)Math.Ceiling((float)imageHeight / renderCountZ);

            float cameraPosX = (float)distX / 2 / renderCountX;
            float cameraPosZ = (float)distZ / 2 / renderCountZ;
            float orthographicSize = Math.Min(cameraPosX, cameraPosZ);
            orthographicSize = cameraPosZ;
            Vector3 cameraBasePos = new Vector3(cameraPosX, 15f + (orthographicSize - 11f) / 49f * 50f, cameraPosZ);

            RenderTexture renderTexture = new RenderTexture(renderWidth, renderHeight, 24);
            if (imageTexture == null)
            {
                imageTexture = new Texture2D(0, 0, TextureFormat.RGB24, false);
            }
            if (imageTexture.width != imageWidth || imageTexture.height != imageHeight)
            {
                imageTexture.Resize(imageWidth, imageHeight);
            }

            Camera camera = Find.Camera;
            CameraDriver camDriver = camera.GetComponent<CameraDriver>();
            camDriver.enabled = false;

            // Store current camera data
            Vector3 rememberedRootPos = map.rememberedCameraPos.rootPos;
            float rememberedRootSize = map.rememberedCameraPos.rootSize;
            float rememberedFarClipPlane = camera.farClipPlane;

            // Overwrite current view rect in the camera driver
            Traverse.Create(camDriver).Field("lastViewRect").SetValue(new CellRect(startX, startZ, endX, endZ));
            yield return new WaitForEndOfFrame();

            // Set camera values needed for rendering
            camera.orthographicSize = orthographicSize;
            camera.farClipPlane = cameraBasePos.y + 6.5f;
            
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
                    camera.transform.position = new Vector3(startX + cameraBasePos.x * (2 * j + 1), cameraBasePos.y, startZ + cameraBasePos.z * (2 * i + 1));
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
            File.WriteAllBytes(CreateCurrentFilePath(map), encodedImage);
            if (PRModSettings.saveMemory)
            {
                imageTexture.Resize(0, 0);
            }

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

        private string CreateCurrentFilePath(Map map)
        {
            // Build image name
            int tick = Find.TickManager.TicksAbs;
            float longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
            int year = GenDate.Year(tick, longitude);
            int quadrum = MoreGenDate.QuadrumInteger(tick, longitude);
            int day = GenDate.DayOfQuadrum(tick, longitude) + 1;
            int hour = GenDate.HourInteger(tick, longitude);
            string imageName = "rimworld-" + Find.World.info.seedString + "-" + map.Tile + "-" + year + "-" + quadrum + "-" + ((day < 10) ? "0" : "") + day + "-" + ((hour < 10) ? "0" : "") + hour;
            // Get correct file and location
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
