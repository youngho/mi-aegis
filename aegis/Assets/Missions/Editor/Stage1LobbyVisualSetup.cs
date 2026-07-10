#if UNITY_EDITOR
#nullable enable
using System.IO;
using PinkSoft.Aegis.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>Stage1 로비 비주얼 — Nexa Core 레퍼런스 룩 (텍스처·머티리얼·조명).</summary>
    public static class Stage1LobbyVisualSetup
    {
        const string Stage1ScenePath = "Assets/Scenes/Stages/Stage1_Lobby.unity";
        const string TexFolder = "Assets/Textures";
        const string MatFolder = "Assets/Materials";

        [MenuItem("Aegis/Apply Stage1 Lobby Visuals")]
        public static void ApplyFromMenu()
        {
            if (!File.Exists(Stage1ScenePath))
            {
                Debug.LogError($"[Stage1LobbyVisualSetup] Missing {Stage1ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
            ApplyToActiveScene();
            Stage1LobbyArchitectureSetup.BuildArchitecture(GameObject.Find("Stage1_Lobby")!.transform);
            Stage1LobbyExteriorSetup.BuildExteriorAndPostProcess();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("[Stage1LobbyVisualSetup] Stage1 lobby visuals applied.");
        }

        public static void ApplyToActiveScene()
        {
            EnsureTexturesImported();
            var floor = EnsureLitMat("M_Lobby_Floor", $"{TexFolder}/T_Lobby_Marble_Floor.png", 0f, 0.92f, new Vector2(8, 8));
            var wall = EnsureLitMat("M_Lobby_Wall", $"{TexFolder}/T_Lobby_Wall_Panel.png", 0f, 0.15f, new Vector2(3, 2));
            wall.SetColor("_BaseColor", new Color(0.88f, 0.88f, 0.88f, 1f));
            var ceiling = EnsureLitMat("M_Lobby_Ceiling", $"{TexFolder}/T_Lobby_Ceiling.png", 0f, 0.15f, new Vector2(4, 4));
            
            // Stone material for counters/backpanel (Plaza Stone)
            var deskStone = EnsureLitMat("M_Lobby_Desk_Stone", $"{TexFolder}/T_Lobby_Plaza_Stone.png", 0f, 0.5f, new Vector2(2, 2));
            deskStone.SetColor("_BaseColor", new Color(0.9f, 0.9f, 0.9f, 1f));

            // Wood material for desk base (Dark Walnut brown)
            var deskWood = EnsureLitMat("M_Lobby_Desk_Wood", null, 0f, 0.18f, Vector2.one);
            deskWood.SetColor("_BaseColor", new Color(0.24f, 0.16f, 0.11f, 1f));

            var column = EnsureLitMat("M_Lobby_Column", $"{TexFolder}/T_Lobby_Column_Marble.png", 0f, 0.88f, new Vector2(1, 2));
            column.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));

            var sign = EnsureEmissiveMat("M_Lobby_Sign", $"{TexFolder}/T_Lobby_NexaCore_Sign.png", 2.5f);
            var screen = EnsureEmissiveMat("M_Lobby_Aegis_Screen", $"{TexFolder}/T_Lobby_Aegis_Screen.png", 1.8f);

            AssignMaterialInPrefabs(floor, wall, ceiling, deskStone, deskWood, column, sign);
            AssignMaterialsInScene(floor, wall, ceiling, deskStone, deskWood, column, sign, screen);
            SetupLightingAndProbes();
        }

        static void EnsureTexturesImported()
        {
            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles($"{TexFolder}", "T_Lobby_*.png"))
            {
                var importer = AssetImporter.GetAtPath(ToAssetPath(file)) as TextureImporter;
                if (importer == null)
                    continue;

                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }
        }

        static string ToAssetPath(string fullPath) =>
            fullPath.Replace('\\', '/').Replace(Application.dataPath, "Assets");

        static Material EnsureLitMat(string matName, string? texPath, float metallic, float smoothness, Vector2 tiling)
        {
            var matPath = $"{MatFolder}/{matName}.mat";
            Texture2D? tex = null;
            if (!string.IsNullOrEmpty(texPath))
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
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
            mat.SetColor("_BaseColor", Color.white);
            mat.EnableKeyword("_ENVIRONMENTREFLECTIONS_ON");
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Material EnsureEmissiveMat(string matName, string texPath, float emissionStrength)
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
                mat.SetTexture("_BaseMap", tex);

            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0.35f);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            var emission = Color.white * emissionStrength;
            mat.SetColor("_EmissionColor", emission);
            if (tex != null)
                mat.SetTexture("_EmissionMap", tex);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void AssignMaterialInPrefabs(Material floor, Material wall, Material ceiling, Material deskStone, Material deskWood, Material column, Material sign)
        {
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Floor.prefab", floor);
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Wall_Front.prefab", wall);
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Wall_Back.prefab", wall);
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Wall_Left.prefab", wall);
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Wall_Right.prefab", wall);
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Ceiling.prefab", ceiling);
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Column.prefab", column);
            SetPrefabMat("Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_Sign.prefab", sign);

            var deskPath = "Assets/Prefabs/BuildingKit/Stage1Lobby/PF_Lobby_ReceptionDesk.prefab";
            var root = PrefabUtility.LoadPrefabContents(deskPath);
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var n = r.gameObject.name;
                if (n.Contains("Countertop") || n.Contains("BackPanel"))
                    r.sharedMaterial = deskStone;
                else
                    r.sharedMaterial = deskWood;
            }

            PrefabUtility.SaveAsPrefabAsset(root, deskPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void SetPrefabMat(string prefabPath, Material mat)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (var i = 0; i < mats.Length; i++)
                    mats[i] = mat;
                r.sharedMaterials = mats;
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void AssignMaterialsInScene(Material floor, Material wall, Material ceiling, Material deskStone, Material deskWood, Material column, Material sign, Material screen)
        {
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                var n = r.gameObject.name;
                var parent = r.transform.parent != null ? r.transform.parent.name : "";

                if (n.Contains("Floor") || parent.Contains("Floor"))
                    r.sharedMaterial = floor;
                else if (n.Contains("Ceiling") || parent.Contains("Ceiling"))
                    r.sharedMaterial = ceiling;
                else if (n.Contains("Wall") || parent.Contains("Wall"))
                    r.sharedMaterial = wall;
                else if (n.Contains("Column") || parent.Contains("Column"))
                    r.sharedMaterial = column;
                else if (n.Contains("Desk") || n.Contains("Reception") || parent.Contains("Reception"))
                {
                    if (n.Contains("Countertop") || n.Contains("BackPanel"))
                        r.sharedMaterial = deskStone;
                    else
                        r.sharedMaterial = deskWood;
                }
                else if (n.Contains("Sign") || parent.Contains("Sign"))
                    r.sharedMaterial = sign;
            }

            EnsureAegisScreen(screen);
            EnsureLoungeChairs();
            FixSignRotationsInScene();
        }

        static void FixSignRotationsInScene()
        {
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t.name == "Sign_Text")
                {
                    t.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    t.localScale = new Vector3(-1f, 1f, 1f);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
                else if (t.name == "ExteriorSign_NexaCore")
                {
                    t.localRotation = Quaternion.Euler(0f, 0f, 180f);
                    t.localScale = new Vector3(8f, t.localScale.y, t.localScale.z);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }
        }

        static void EnsureAegisScreen(Material screenMat)
        {
            var env = GameObject.Find("Environment_Stage1_Lobby") ?? GameObject.Find("Stage1_Lobby");
            if (env == null)
                return;

            var existing = env.transform.Find("Aegis_InfoScreen");
            if (existing == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Aegis_InfoScreen";
                go.transform.SetParent(env.transform, false);
                go.transform.localPosition = new Vector3(12f, 2.8f, 0f);
                go.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                go.transform.localScale = new Vector3(0.08f, 2.2f, 3.8f);
                Object.DestroyImmediate(go.GetComponent<BoxCollider>());
                existing = go.transform;
            }

            var renderer = existing.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = screenMat;
        }

        static void EnsureLoungeChairs()
        {
            var env = GameObject.Find("Environment_Stage1_Lobby");
            if (env == null)
                return;

            if (env.transform.Find("LoungeChairs") != null)
                return;

            var root = new GameObject("LoungeChairs");
            root.transform.SetParent(env.transform, false);

            var chairMat = EnsureLitMat("M_Lobby_Chair", null, 0f, 0.2f, Vector2.one);
            chairMat.SetColor("_BaseColor", new Color(0.12f, 0.13f, 0.15f));

            var positions = new[]
            {
                new Vector3(-8f, 0.35f, -2f),
                new Vector3(-6.5f, 0.35f, -2f),
                new Vector3(6.5f, 0.35f, -2f),
                new Vector3(8f, 0.35f, -2f),
            };

            foreach (var pos in positions)
            {
                var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.name = "LoungeChair";
                chair.transform.SetParent(root.transform, false);
                chair.transform.localPosition = pos;
                chair.transform.localScale = new Vector3(1.1f, 0.7f, 1f);
                chair.GetComponent<Renderer>().sharedMaterial = chairMat;
            }
        }

        static void SetupLightingAndProbes()
        {
            var sun = Object.FindAnyObjectByType<Light>();
            if (sun != null && sun.type == LightType.Directional)
            {
                sun.intensity = 0.85f;
                sun.color = new Color(0.95f, 0.97f, 1f);
                sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
                sun.shadows = LightShadows.Soft;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.82f, 0.85f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.68f, 0.7f, 0.74f);
            RenderSettings.ambientGroundColor = new Color(0.48f, 0.5f, 0.52f);
            RenderSettings.reflectionIntensity = 1.0f;

            EnsureReflectionProbe(
                new Vector3(0f, Stage1LobbyDimensions.WallCenterY, 0f),
                new Vector3(28f, Stage1LobbyDimensions.CeilingHeight + 2f, 28f));
            EnsureAccentLight("Lobby_SignLight", new Vector3(0f, Stage1LobbyDimensions.BackSignCenterY, 11f), new Color(0.9f, 0.95f, 1f), 3.0f, 15f);
            EnsureAccentLight("Lobby_ReceptionLight", new Vector3(0f, Stage1LobbyDimensions.WallCenterY * 0.6f, -4f), new Color(1f, 1f, 1f), 2.5f, 12f);
            EnsureAccentLight("Lobby_EntranceLight", new Vector3(0f, Stage1LobbyDimensions.WallCenterY * 0.55f, -13f), new Color(0.95f, 0.98f, 1f), 2.2f, 10f);
        }

        static void EnsureReflectionProbe(Vector3 pos, Vector3 size)
        {
            var existing = GameObject.Find("Lobby_ReflectionProbe");
            if (existing == null)
            {
                existing = new GameObject("Lobby_ReflectionProbe");
                existing.AddComponent<ReflectionProbe>();
            }

            existing.transform.position = pos;
            var probe = existing.GetComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.size = size;
            probe.boxProjection = true;
            probe.intensity = 1f;
            probe.RenderProbe();
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
