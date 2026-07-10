#if UNITY_EDITOR
#nullable enable
using System.IO;
using PinkSoft.Aegis.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>Stage1 로비 외관 — ProBuilder 입구·유리·플라자·도시 배경·Bloom.</summary>
    public static class Stage1LobbyExteriorSetup
    {
        const string Stage1ScenePath = "Assets/Scenes/Stages/Stage1_Lobby.unity";
        const string VolumeProfilePath = "Assets/Settings/Stage1LobbyVolumeProfile.asset";
        const string SkyMatPath = "Assets/Materials/M_Lobby_Sky_Twilight.mat";
        const string TexFolder = "Assets/Textures";
        const string MatFolder = "Assets/Materials";
        const string ExteriorRootName = "Environment_Stage1_Exterior";

        [MenuItem("Aegis/Build Stage1 Lobby Exterior")]
        public static void BuildFromMenu()
        {
            if (!File.Exists(Stage1ScenePath))
            {
                Debug.LogError($"[Stage1LobbyExteriorSetup] Missing {Stage1ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
            Stage1LobbyVisualSetup.ApplyToActiveScene();
            BuildExteriorAndPostProcess();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("[Stage1LobbyExteriorSetup] Exterior, city backdrop, and bloom applied.");
        }

        public static void BuildExteriorAndPostProcess()
        {
            var materials = LoadMaterials();
            SetupSkybox(materials);
            SetupPostProcessingVolume();
            SetupTwilightLighting();

            var stage1 = GameObject.Find("Stage1_Lobby");
            if (stage1 == null)
            {
                Debug.LogError("[Stage1LobbyExteriorSetup] Stage1_Lobby root not found.");
                return;
            }

            var exterior = GetOrCreateRoot(stage1.transform, ExteriorRootName);
            ClearChildren(exterior.transform);

            HideSolidLeftWall();
            BuildExteriorPlaza(exterior.transform, materials);
            BuildEntranceCanopy(exterior.transform, materials);
            BuildGlassFacade(exterior.transform, materials);
            BuildCityBackdrop(exterior.transform, materials);
            BuildExteriorSign(exterior.transform, materials);
            BuildPlantersAndBenches(exterior.transform, materials);
            BuildPlazaLedStrips(exterior.transform, materials);
            Stage1LobbyVisualSetup.SetupLightingAndProbes();
        }

        struct LobbyMaterials
        {
            public Material floor;
            public Material plaza;
            public Material wall;
            public Material metal;
            public Material glass;
            public Material sign;
            public Material led;
            public Material city;
        }

        static LobbyMaterials LoadMaterials()
        {
            return new LobbyMaterials
            {
                floor = LoadMat("M_Lobby_Floor"),
                plaza = EnsureLitMat("M_Lobby_Plaza", $"{TexFolder}/T_Lobby_Plaza_Stone.png", 0f, 0.82f, new Vector2(6, 6)),
                wall = LoadMat("M_Lobby_Wall"),
                metal = LoadMat("M_Lobby_Desk"),
                glass = EnsureGlassMat(),
                sign = LoadMat("M_Lobby_Sign"),
                led = EnsureEmissiveMat("M_Lobby_LED", $"{TexFolder}/T_Lobby_LED_Strip.png", 3f),
                city = EnsureLitMat("M_Lobby_CityBackdrop", $"{TexFolder}/T_Lobby_City_Twilight.png", 0f, 0.05f, Vector2.one),
            };
        }

        static Material LoadMat(string name) =>
            AssetDatabase.LoadAssetAtPath<Material>($"{MatFolder}/{name}.mat");

        static Material EnsureLitMat(string matName, string texPath, float metallic, float smoothness, Vector2 tiling)
        {
            var matPath = $"{MatFolder}/{matName}.mat";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", tiling);
            }

            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Material EnsureEmissiveMat(string matName, string texPath, float strength)
        {
            var matPath = $"{MatFolder}/{matName}.mat";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTexture("_EmissionMap", tex);
            }

            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", Color.white * strength);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Material EnsureGlassMat()
        {
            var mat = LoadMat("M_Lobby_Glass");
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, $"{MatFolder}/M_Lobby_Glass.mat");
            }

            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            mat.SetColor("_BaseColor", new Color(0.72f, 0.85f, 0.95f, 0.22f));
            mat.SetFloat("_Smoothness", 0.96f);
            mat.SetFloat("_Metallic", 0.15f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void SetupSkybox(LobbyMaterials materials)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexFolder}/T_Lobby_Sky_Twilight.png");
            if (tex == null)
                return;

            var mat = AssetDatabase.LoadAssetAtPath<Material>(SkyMatPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Skybox/Panoramic"));
                AssetDatabase.CreateAsset(mat, SkyMatPath);
            }

            mat.SetTexture("_MainTex", tex);
            mat.SetFloat("_Exposure", 1.05f);
            mat.SetFloat("_Rotation", 120f);
            RenderSettings.skybox = mat;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.68f, 0.80f);
            RenderSettings.ambientEquatorColor = new Color(0.50f, 0.54f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.32f, 0.34f, 0.38f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.35f, 0.38f, 0.52f);
        }

        static void SetupPostProcessingVolume()
        {
            const string sampleProfilePath = "Assets/Settings/SampleSceneProfile.asset";
            if (!File.Exists(VolumeProfilePath))
                AssetDatabase.CopyAsset(sampleProfilePath, VolumeProfilePath);

            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
                return;

            if (profile.TryGet(out Bloom bloom))
            {
                bloom.active = true;
                bloom.threshold.Override(0.75f);
                bloom.intensity.Override(0.65f);
                bloom.scatter.Override(0.72f);
                bloom.highQualityFiltering.Override(true);
            }

            if (!profile.TryGet(out ColorAdjustments color))
                color = profile.Add<ColorAdjustments>(true);
            color.active = true;
            color.postExposure.Override(0.15f);
            color.contrast.Override(12f);
            color.saturation.Override(8f);
            color.colorFilter.Override(new Color(0.95f, 0.97f, 1f));

            if (profile.TryGet(out Vignette vignette))
                vignette.intensity.Override(0.18f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var volumeGo = GameObject.Find("Stage1_GlobalVolume");
            if (volumeGo == null)
            {
                volumeGo = new GameObject("Stage1_GlobalVolume");
                volumeGo.AddComponent<Volume>();
            }

            var volume = volumeGo.GetComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.profile = profile;
        }

        static void SetupTwilightLighting()
        {
            var sun = Stage1LobbyVisualSetup.EnsureDirectionalSun();
            sun.intensity = 0.75f;
            sun.color = new Color(0.82f, 0.88f, 1f);
            sun.transform.rotation = Quaternion.Euler(18f, 145f, 0f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.65f;

            EnsureAccentLight("Lobby_ExteriorRim", new Vector3(-8f, Stage1LobbyDimensions.ExteriorRimLightY, -35f), new Color(0.55f, 0.75f, 1f), 2.5f, 35f);
        }

        static void HideSolidLeftWall()
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (t.name.Contains("Wall_Left") || t.name.Contains("PF_Lobby_Wall_Left"))
                    t.gameObject.SetActive(false);
            }
        }

        static void BuildExteriorPlaza(Transform parent, LobbyMaterials m)
        {
            var plaza = CreatePbCube("Plaza_Exterior", parent, new Vector3(0f, -0.05f, -40f), new Vector3(64f, 0.1f, 20f));
            SetRendererMat(plaza, m.plaza);

            // plaza steps / threshold at entrance
            var threshold = CreatePbCube("Plaza_Threshold", parent, new Vector3(0f, 0.05f, -30.6f), new Vector3(16f, 0.08f, 1.2f));
            SetRendererMat(threshold, m.plaza);
        }

        static void BuildEntranceCanopy(Transform parent, LobbyMaterials m)
        {
            var canopy = CreatePbCube("Entrance_Canopy", parent, new Vector3(0f, Stage1LobbyDimensions.ExteriorCanopyY, -30.8f), new Vector3(18f, 0.35f, 4.5f));
            SetRendererMat(canopy, m.metal);

            var frameL = CreatePbCube("Entrance_Frame_L", parent, new Vector3(-9.0f, Stage1LobbyDimensions.ExteriorFrameCenterY, -30.7f), new Vector3(0.35f, Stage1LobbyDimensions.ExteriorFrameHeight, 0.35f));
            var frameR = CreatePbCube("Entrance_Frame_R", parent, new Vector3(9.0f, Stage1LobbyDimensions.ExteriorFrameCenterY, -30.7f), new Vector3(0.35f, Stage1LobbyDimensions.ExteriorFrameHeight, 0.35f));
            SetRendererMat(frameL, m.metal);
            SetRendererMat(frameR, m.metal);

            // revolving door cylinder (simplified)
            var doorPb = ShapeGenerator.CreateShape(ShapeType.Cylinder);
            var door = doorPb.gameObject;
            door.name = "RevolvingDoor_Glass";
            door.transform.SetParent(parent, false);
            door.transform.localPosition = new Vector3(0f, Stage1LobbyDimensions.ExteriorDoorCenterY, -29.6f);
            door.transform.localScale = new Vector3(3.2f, Stage1LobbyDimensions.ExteriorDoorScaleY, 3.2f);
            SetRendererMat(door, m.glass);
        }

        static void BuildGlassFacade(Transform parent, LobbyMaterials m)
        {
            var facade = new GameObject("GlassFacade_Left");
            facade.transform.SetParent(parent, false);

            var panels = new[]
            {
                new Vector3(-29.85f, Stage1LobbyDimensions.GlassFacadeCenterY, -20f),
                new Vector3(-29.85f, Stage1LobbyDimensions.GlassFacadeCenterY, -10f),
                new Vector3(-29.85f, Stage1LobbyDimensions.GlassFacadeCenterY, 0f),
                new Vector3(-29.85f, Stage1LobbyDimensions.GlassFacadeCenterY, 10f),
                new Vector3(-29.85f, Stage1LobbyDimensions.GlassFacadeCenterY, 20f),
            };

            foreach (var pos in panels)
            {
                var panel = CreatePbCube("GlassPanel", facade.transform, pos, new Vector3(0.08f, Stage1LobbyDimensions.GlassPanelHeight, 10.0f));
                SetRendererMat(panel, m.glass);
            }

            // mullions
            for (var z = -25f; z <= 25f; z += 10.0f)
            {
                var mullion = CreatePbCube("Mullion", facade.transform, new Vector3(-29.82f, Stage1LobbyDimensions.GlassFacadeCenterY, z), new Vector3(0.12f, Stage1LobbyDimensions.GlassPanelHeight + 0.1f, 0.15f));
                SetRendererMat(mullion, m.metal);
            }
        }

        static void BuildCityBackdrop(Transform parent, LobbyMaterials m)
        {
            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "CityBackdrop_Twilight";
            Object.DestroyImmediate(backdrop.GetComponent<Collider>());
            backdrop.transform.SetParent(parent, false);
            backdrop.transform.localPosition = new Vector3(-45f, Stage1LobbyDimensions.CityBackdropY, 2f);
            backdrop.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            backdrop.transform.localScale = new Vector3(70f, 18f * Stage1LobbyDimensions.ScaleFromLegacy, 1f);
            backdrop.GetComponent<Renderer>().sharedMaterial = m.city;

            // secondary backdrop visible through entrance
            var ext = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ext.name = "CityBackdrop_Entrance";
            Object.DestroyImmediate(ext.GetComponent<Collider>());
            ext.transform.SetParent(parent, false);
            ext.transform.localPosition = new Vector3(0f, Stage1LobbyDimensions.CityBackdropEntranceY, -45f);
            ext.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            ext.transform.localScale = new Vector3(70f, 14f * Stage1LobbyDimensions.ScaleFromLegacy, 1f);
            ext.GetComponent<Renderer>().sharedMaterial = m.city;
        }

        static void BuildExteriorSign(Transform parent, LobbyMaterials m)
        {
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "ExteriorSign_NexaCore";
            Object.DestroyImmediate(sign.GetComponent<Collider>());
            sign.transform.SetParent(parent, false);
            sign.transform.localPosition = new Vector3(0f, Stage1LobbyDimensions.ExteriorSignY, -30.55f);
            sign.transform.localScale = new Vector3(12f, 2.2f * Stage1LobbyDimensions.ScaleFromLegacy, 0.12f);
            sign.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            sign.GetComponent<Renderer>().sharedMaterial = m.sign;

            EnsureAccentLight("Lobby_ExteriorSignLight", new Vector3(0f, Stage1LobbyDimensions.ExteriorSignY, -29f), new Color(0.9f, 0.95f, 1f), 3f, 14f);
        }

        static void BuildPlantersAndBenches(Transform parent, LobbyMaterials m)
        {
            var root = new GameObject("Plaza_Furnishing");
            root.transform.SetParent(parent, false);

            var planterPositions = new[]
            {
                new Vector3(-15f, 0.35f, -34f),
                new Vector3(15f, 0.35f, -34f),
                new Vector3(-15f, 0.35f, -38f),
                new Vector3(15f, 0.35f, -38f),
            };

            foreach (var pos in planterPositions)
            {
                var planter = CreatePbCube("Planter", root.transform, pos, new Vector3(2.2f, 0.7f, 2.2f));
                SetRendererMat(planter, m.wall);
                var led = CreatePbCube("Planter_LED", root.transform, pos + new Vector3(0f, 0.38f, 0f), new Vector3(2.3f, 0.06f, 2.3f));
                SetRendererMat(led, m.led);
            }

            var benchMat = EnsureLitMat("M_Lobby_Bench", null, 0f, 0.35f, Vector2.one);
            benchMat.SetColor("_BaseColor", new Color(0.55f, 0.56f, 0.58f));

            var benchPos = new[] { new Vector3(-8f, 0.25f, -35f), new Vector3(8f, 0.25f, -35f) };
            foreach (var pos in benchPos)
            {
                var bench = CreatePbCube("Plaza_Bench", root.transform, pos, new Vector3(3.2f, 0.5f, 0.9f));
                SetRendererMat(bench, benchMat);
            }
        }

        static void BuildPlazaLedStrips(Transform parent, LobbyMaterials m)
        {
            var stripRoot = new GameObject("Plaza_LED_Strips");
            stripRoot.transform.SetParent(parent, false);

            for (var x = -24f; x <= 24f; x += 8f)
            {
                var strip = CreatePbCube("FloorLED", stripRoot.transform, new Vector3(x, 0.02f, -32.5f), new Vector3(6.5f, 0.02f, 0.15f));
                SetRendererMat(strip, m.led);
            }
        }

        static GameObject CreatePbCube(string name, Transform parent, Vector3 localPos, Vector3 localScale)
        {
            var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
            var go = pb.gameObject;
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            return go;
        }

        static void SetRendererMat(GameObject go, Material mat)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = mat;
        }

        static GameObject GetOrCreateRoot(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            return go;
        }

        static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        static void EnsureAccentLight(string name, Vector3 pos, Color color, float intensity, float range)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                go.AddComponent<Light>();
            }

            go.transform.position = pos;
            var light = go.GetComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }
    }
}
#endif
