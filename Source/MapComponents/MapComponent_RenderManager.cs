using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluxJpeg.Core;
using FluxJpeg.Core.Encoder;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ProgressRenderer
{

    public class MapComponent_RenderManager : MapComponent
    {

        private const int RenderTextureSize = 4096;

        public static int nextGlobalRenderManagerTickOffset = 0;

        private int tickOffset = -1;
        private int lastRenderedHour = -999;
        private int lastRenderedCounter = 0;
        private float rsOldStartX = -1f;
        private float rsOldStartZ = -1f;
        private float rsOldEndX = -1f;
        private float rsOldEndZ = -1f;
        private float rsTargetStartX = -1f;
        private float rsTargetStartZ = -1f;
        private float rsTargetEndX = -1f;
        private float rsTargetEndZ = -1f;
        private float rsCurrentPosition = 1f;

        private Texture2D imageTexture;
        private byte[] imageTextureRawData;
        private int imageTextureWidth;
        private int imageTextureHeight;

        private bool manuallyTriggered = false;
        private bool renderingInt = false;
        private bool encodingInt = false;
        Thread encodeThread;
        private bool ctrlEncodingPost = false;
        private SmallMessageBox messageBox;

        public MapComponent_RenderManager(Map map) : base(map)
        {
        }

        public bool Rendering
        {
            get
            {
                return renderingInt;
            }
        }

        public override void FinalizeInit() // New map and after loading
        {
            if (tickOffset < 0)
            {
                tickOffset = nextGlobalRenderManagerTickOffset;
                nextGlobalRenderManagerTickOffset = (nextGlobalRenderManagerTickOffset + 5) % GenTicks.TickRareInterval;
            }
        }

        /*
        public override void MapGenerated() // Only new map
        {
        }
        */

        public override void MapComponentUpdate()
        {
            // Watch for finished encoding to clean up and free memory
            if (ctrlEncodingPost)
            {
                DoEncodingPost();
                ctrlEncodingPost = false;
            }
        }

        public override void MapComponentTick()
        {
            // TickRare
            if (Find.TickManager.TicksGame % 250 != tickOffset)
            {
                return;
            }
            // Check for rendering
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
            // Update timing variables
            lastRenderedHour = currHour;
            lastRenderedCounter++;
            // Check if rendering is enabled
            if (!PRModSettings.enabled)
            {
                return;
            }
            // Show message window or print message
            ShowCurrentRenderMessage();
            // Start rendering
            Find.CameraDriver.StartCoroutine(DoRendering());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastRenderedHour, "lastRenderedHour", -999);
            Scribe_Values.Look(ref lastRenderedCounter, "lastRenderedCounter", 0);
            Scribe_Values.Look(ref rsOldStartX, "rsOldStartX", -1f);
            Scribe_Values.Look(ref rsOldStartZ, "rsOldStartZ", -1f);
            Scribe_Values.Look(ref rsOldEndX, "rsOldEndX", -1f);
            Scribe_Values.Look(ref rsOldEndZ, "rsOldEndZ", -1f);
            Scribe_Values.Look(ref rsTargetStartX, "rsTargetStartX", -1f);
            Scribe_Values.Look(ref rsTargetStartZ, "rsTargetStartZ", -1f);
            Scribe_Values.Look(ref rsTargetEndX, "rsTargetEndX", -1f);
            Scribe_Values.Look(ref rsTargetEndZ, "rsTargetEndZ", -1f);
            Scribe_Values.Look(ref rsCurrentPosition, "rsCurrentPosition", 1f);
        }

        public static void TriggerCurrentMapManualRendering(bool forceRenderFullMap = false)
        {
            Find.CurrentMap.GetComponent<MapComponent_RenderManager>().DoManualRendering(forceRenderFullMap);
        }

        public void DoManualRendering(bool forceRenderFullMap = false)
        {
            ShowCurrentRenderMessage();
            manuallyTriggered = true;
            Find.CameraDriver.StartCoroutine(DoRendering(forceRenderFullMap));
        }

        private void ShowCurrentRenderMessage()
        {
            if (PRModSettings.renderFeedback == RenderFeedback.Window && PRModSettings.encoding != "jpg_fluxthreaded")
            {
                messageBox = new SmallMessageBox("LPR_Rendering".Translate());
                Find.WindowStack.Add(messageBox);
            }
            else if (PRModSettings.renderFeedback == RenderFeedback.Message)
            {
                Messages.Message("LPR_Rendering".Translate(), MessageTypeDefOf.CautionInput, false);
            }
        }

        private IEnumerator DoRendering(bool forceRenderFullMap = false)
        {
            yield return new WaitForFixedUpdate();
            if (renderingInt)
            {
                Log.Error("Progress Renderer is still rendering an image while a new rendering was requested. This can lead to missing or wrong data. (This can also happen in rare situations when you trigger manual rendering the exact same time as an automatic rendering happens. If you did that, just check your export folder if both renderings were done corrently and ignore this error.)");
            }
            renderingInt = true;

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
            float startX = 0;
            float startZ = 0;
            float endX = map.Size.x;
            float endZ = map.Size.z;
            if (!forceRenderFullMap)
            {
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
            }

            // Only use smoothing when rendering was not triggered manually
            if (!manuallyTriggered)
            {
                // Test if target render area changed to reset smoothing
                if (rsTargetStartX != startX || rsTargetStartZ != startZ || rsTargetEndX != endX || rsTargetEndZ != endZ)
                {
                    // Check if area was manually reset or uninitialized (-1) to not smooth
                    if (rsTargetStartX == -1f && rsTargetStartZ == -1f && rsTargetEndX == -1f && rsTargetEndZ == -1f)
                    {
                        rsCurrentPosition = 1f;
                    }
                    else
                    {
                        rsCurrentPosition = 1f / (PRModSettings.smoothRenderAreaSteps + 1);
                    }
                    rsOldStartX = rsTargetStartX;
                    rsOldStartZ = rsTargetStartZ;
                    rsOldEndX = rsTargetEndX;
                    rsOldEndZ = rsTargetEndZ;
                    rsTargetStartX = startX;
                    rsTargetStartZ = startZ;
                    rsTargetEndX = endX;
                    rsTargetEndZ = endZ;
                }
                // Apply smoothing to render area
                if (rsCurrentPosition < 1f)
                {
                    startX = rsOldStartX + (rsTargetStartX - rsOldStartX) * rsCurrentPosition;
                    startZ = rsOldStartZ + (rsTargetStartZ - rsOldStartZ) * rsCurrentPosition;
                    endX = rsOldEndX + (rsTargetEndX - rsOldEndX) * rsCurrentPosition;
                    endZ = rsOldEndZ + (rsTargetEndZ - rsOldEndZ) * rsCurrentPosition;
                    rsCurrentPosition += 1f / (PRModSettings.smoothRenderAreaSteps + 1);
                }
            }

            float distX = endX - startX;
            float distZ = endZ - startZ;
            
            // Calculate basic values that are used for rendering
            int imageWidth;
            int imageHeight;
            if (PRModSettings.outputImageFixedHeight > 0)
            {
                imageHeight = PRModSettings.outputImageFixedHeight;
                imageWidth = (int)((float)imageHeight / distZ * distX);
            }
            else
            {
                imageWidth = (int)(distX * PRModSettings.pixelPerCell);
                imageHeight = (int)(distZ * PRModSettings.pixelPerCell);
            }

            int renderCountX = (int)Math.Ceiling((float)imageWidth / RenderTextureSize);
            int renderCountZ = (int)Math.Ceiling((float)imageHeight / RenderTextureSize);
            int renderWidth = (int)Math.Ceiling((float)imageWidth / renderCountX);
            int renderHeight = (int)Math.Ceiling((float)imageHeight / renderCountZ);

            float cameraPosX = (float)distX / 2 / renderCountX;
            float cameraPosZ = (float)distZ / 2 / renderCountZ;
            float orthographicSize = Math.Min(cameraPosX, cameraPosZ);
            orthographicSize = cameraPosZ;
            Vector3 cameraBasePos = new Vector3(cameraPosX, 15f + (orthographicSize - 11f) / 49f * 50f, cameraPosZ);

            RenderTexture renderTexture = RenderTexture.GetTemporary(renderWidth, renderHeight, 24);
            imageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            
            Camera camera = Find.Camera;
            CameraDriver camDriver = camera.GetComponent<CameraDriver>();
            camDriver.enabled = false;

            // Store current camera data
            Vector3 rememberedRootPos = map.rememberedCameraPos.rootPos;
            float rememberedRootSize = map.rememberedCameraPos.rootSize;
            float rememberedFarClipPlane = camera.farClipPlane;

            // Overwrite current view rect in the camera driver
            CellRect camViewRect = camDriver.CurrentViewRect;
            int camRectMinX = Math.Min((int)startX, camViewRect.minX);
            int camRectMinZ = Math.Min((int)startZ, camViewRect.minZ);
            int camRectMaxX = Math.Max((int)Math.Ceiling(endX), camViewRect.maxX);
            int camRectMaxZ = Math.Max((int)Math.Ceiling(endZ), camViewRect.maxZ);
            Traverse camDriverTraverse = Traverse.Create(camDriver);
            camDriverTraverse.Field("lastViewRect").SetValue(CellRect.FromLimits(camRectMinX, camRectMinZ, camRectMaxX, camRectMaxZ));
            camDriverTraverse.Field("lastViewRectGetFrame").SetValue(Time.frameCount);
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

            // Restore camera and viewport
            RenderTexture.active = null;
            //tmpCam.targetTexture = null;
            camera.targetTexture = null;
            camera.farClipPlane = rememberedFarClipPlane;
            camDriver.SetRootPosAndSize(rememberedRootPos, rememberedRootSize);
            camDriver.enabled = true;

            RenderTexture.ReleaseTemporary(renderTexture);

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

            // Sinal finished rendering
            renderingInt = false;
            yield return null;

            // Start encoding
            DoEncoding();

            yield break;
        }

        private void DoEncoding()
        {
            if (encodingInt)
            {
                Log.Error("Progress Renderer is still encoding an image while the encoder was called again. This can lead to missing or wrong data.");
            }
            encodingInt = true;
            if (PRModSettings.encoding == "png_unity")
            {
                EncodeUnityPng();
            }
            else if (PRModSettings.encoding == "jpg_unity")
            {
                EncodeUnityJpg();
            }
            else if (PRModSettings.encoding == "jpg_fluxthreaded")
            {
                // Prepare data
                imageTextureRawData = imageTexture.GetRawTextureData();
                imageTextureWidth = imageTexture.width;
                imageTextureHeight = imageTexture.height;
                // Start thread
                encodeThread = new Thread(EncodeFluxJpeg);
                encodeThread.Start();
            }
            else
            {
                Log.Error("Progress Renderer encoding setting is wrong or missing. Using default for now. Go to the settings and set a new value.");
                EncodeUnityJpg();
            }
        }

        private void DoEncodingPost()
        {
            // Clean up unused objects
            imageTextureRawData = null;
            UnityEngine.Object.Destroy(imageTexture);
            imageTextureWidth = 0;
            imageTextureHeight = 0;
            
            // Signal finished encoding
            manuallyTriggered = false;
            encodingInt = false;

            // Hide message box
            if (messageBox != null)
            {
                messageBox.Close();
                messageBox = null;
            }
        }

        private void EncodeUnityPng()
        {
            byte[] encodedImage = imageTexture.EncodeToPNG();
            SaveUnityEncoding(encodedImage);
        }

        private void EncodeUnityJpg()
        {
            byte[] encodedImage = imageTexture.EncodeToJPG();
            SaveUnityEncoding(encodedImage);
        }

        private void SaveUnityEncoding(byte[] encodedImage)
        {
            // Create file and save encoded image
            string filePath = CreateCurrentFilePath();
            File.WriteAllBytes(filePath, encodedImage);
            // Create tmp copy to file if needed
            if (!manuallyTriggered && PRModSettings.fileNamePattern == FileNamePattern.BothTmpCopy)
            {
                File.Copy(filePath, CreateFilePath(FileNamePattern.Numbered, true));
            }
            DoEncodingPost();
        }

        private void EncodeFluxJpeg()
        {
            Log.Message("0 - start");
            // Get all needed data which could collide as fast as possible
            bool localManuallyTriggered = manuallyTriggered;
            string filePath = CreateCurrentFilePath();
            // Convert temp data to local raw data
            byte[][,] rawImage = new byte[3][,];
            Log.Message("1");
            rawImage[0] = new byte[imageTextureWidth, imageTextureHeight];
            rawImage[1] = new byte[imageTextureWidth, imageTextureHeight];
            rawImage[2] = new byte[imageTextureWidth, imageTextureHeight];
            Log.Message("1");
            for (int row = 0; row < imageTextureHeight; row++)
            {
                for (int col = 0; col < imageTextureWidth; col++)
                {
                    int index = ((imageTextureHeight - 1 - row) * imageTextureWidth + col) * 3;
                    rawImage[0][col, row] = imageTextureRawData[index];
                    rawImage[1][col, row] = imageTextureRawData[index + 1];
                    rawImage[2][col, row] = imageTextureRawData[index + 2];
                }
            }
            Log.Message("3 - post raw");
            // Tmp cleanup
            ctrlEncodingPost = true;
            // Encode raw data and save the final image
            ColorModel model = new ColorModel { colorspace = FluxJpeg.Core.ColorSpace.RGB };
            Log.Message("4");
            FluxJpeg.Core.Image image = new FluxJpeg.Core.Image(model, rawImage);
            Log.Message("5 - post image");
            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            Log.Message("6 . post fs");
            JpegEncoder encoder = new JpegEncoder(image, 75, fileStream);
            Log.Message("7 - post encode");
            encoder.Encode();
            Log.Message("8 - post save");
            // Local cleanup
            fileStream.Dispose();
            image = null;
            rawImage = null;
            Log.Message("end - 9");
            // Create tmp copy to file if needed
            if (!localManuallyTriggered && PRModSettings.fileNamePattern == FileNamePattern.BothTmpCopy)
            {
                File.Copy(filePath, CreateFilePath(FileNamePattern.Numbered, true));
            }
        }

        private string CreateCurrentFilePath()
        {
            return CreateFilePath(manuallyTriggered ? FileNamePattern.DateTime : PRModSettings.fileNamePattern);
        }

        private string CreateFilePath(FileNamePattern fileNamePattern, bool addTmpSubdir = false)
        {
            // Build image name
            string imageName;
            if (fileNamePattern == FileNamePattern.Numbered)
            {
                imageName = CreateImageNameNumbered();
            }
            else
            {
                imageName = CreateImageNameDateTime();
            }
            // Create path and subdirectory
            string path = PRModSettings.exportPath;
            if (PRModSettings.createSubdirs)
            {
                path = Path.Combine(path, Find.World.info.seedString);
            }
            Directory.CreateDirectory(path);
            // Add subdir for manually triggered renderings
            if (manuallyTriggered)
            {
                path = Path.Combine(path, "manually");
                Directory.CreateDirectory(path);
            }
            // Create additional subdir for numbered symlinks
            if (addTmpSubdir)
            {
                path = Path.Combine(path, "tmp");
                Directory.CreateDirectory(path);
            }
            // Get correct file and location
            string fileExt = PRModSettings.encoding.Substring(0, 3);
            string filePath = Path.Combine(path, imageName + "." + fileExt);
            if (!File.Exists(filePath))
            {
                return filePath;
            }
            int i = 1;
            filePath = Path.Combine(path, imageName);
            string newPath;
            do
            {
                newPath = filePath + "-alt" + i + "." + fileExt;
                i++;
            }
            while (File.Exists(newPath));
            return newPath;
        }

        private string CreateImageNameDateTime()
        {
            int tick = Find.TickManager.TicksAbs;
            float longitude = Find.WorldGrid.LongLatOf(map.Tile).x;
            int year = GenDate.Year(tick, longitude);
            int quadrum = MoreGenDate.QuadrumInteger(tick, longitude);
            int day = GenDate.DayOfQuadrum(tick, longitude) + 1;
            int hour = GenDate.HourInteger(tick, longitude);
            return "rimworld-" + Find.World.info.seedString + "-" + map.Tile + "-" + year + "-" + quadrum + "-" + ((day < 10) ? "0" : "") + day + "-" + ((hour < 10) ? "0" : "") + hour;
        }

        private string CreateImageNameNumbered()
        {
            return "rimworld-" + Find.World.info.seedString + "-" + map.Tile + "-" + lastRenderedCounter.ToString("000000");
        }

    }

}
