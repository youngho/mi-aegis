#if UNITY_EDITOR
#nullable enable
using PinkSoft.Aegis.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>
    /// docs/stages/stage4_core.md + docs/design/cut_timeline.md 기준 펜트하우스 및 메인프레임 코어 구조물 빌더.
    /// Sketchfab: Futuristic Terminal, SciFi Reactor Core, Badass Exosuit
    /// </summary>
    public static class Stage4CoreArchitectureSetup
    {
        const string Stage4ScenePath = "Assets/Scenes/Stages/Stage4_Core.unity";
        const string ArchitectureRootName = "Architecture_Stage4_Core";
        const string EnvironmentRootName = "Environment_Stage4_Core";
        const string PrefabFolder = "Assets/Prefabs";
        const string Stage3PrefabFolder = "Assets/Prefabs/Stage3_Datacenter";
        const string Stage4PrefabFolder = "Assets/Prefabs/Stage4_Core";
        const string MatFolder = "Assets/Materials";

        [MenuItem("Aegis/Build Stage4 Core Architecture")]
        public static void BuildFromMenu()
        {
            if (!System.IO.File.Exists(Stage4ScenePath))
            {
                Debug.LogError($"[Stage4CoreArchitectureSetup] Missing {Stage4ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage4ScenePath, OpenSceneMode.Single);
            var stage4 = GameObject.Find("Stage4_Core");
            if (stage4 == null)
            {
                Debug.LogError("[Stage4CoreArchitectureSetup] Stage4_Core root not found.");
                return;
            }

            PrepareStageRoot(stage4);
            BuildEnvironment(stage4.transform);
            BuildArchitecture(stage4.transform);
            BuildLighting(stage4.transform);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[Stage4CoreArchitectureSetup] Penthouse office, balcony, security corridor, and mainframe core built.");
        }

        public static void BuildAll(Transform stage4)
        {
            PrepareStageRoot(stage4.gameObject);
            BuildEnvironment(stage4);
            BuildArchitecture(stage4);
            BuildLighting(stage4);
        }

        static void PrepareStageRoot(GameObject stage4)
        {
            foreach (var renderer in stage4.GetComponents<MeshRenderer>())
                Object.DestroyImmediate(renderer);
            foreach (var filter in stage4.GetComponents<MeshFilter>())
                Object.DestroyImmediate(filter);
            foreach (var collider in stage4.GetComponents<Collider>())
                Object.DestroyImmediate(collider);

            if (stage4.GetComponent<StageRoot>() == null)
                stage4.AddComponent<StageRoot>();
        }

        public static void BuildEnvironment(Transform stage4)
        {
            var floorMat = EnsureMat("M_Core_Floor", new Color(0.12f, 0.12f, 0.14f));
            var wallMat = EnsureMat("M_Core_Wall", new Color(0.18f, 0.19f, 0.22f));
            var ceilingMat = EnsureMat("M_Core_Ceiling", new Color(0.08f, 0.08f, 0.1f));
            var metalMat = EnsureMat("M_Core_Metal", new Color(0.25f, 0.27f, 0.3f));
            var glassMat = EnsureMat("M_Core_Glass", new Color(0.2f, 0.7f, 1f, 0.25f));
            var hologramMat = EnsureMat("M_Core_Hologram", new Color(0.1f, 0.8f, 1f, 0.4f));
            SetEmissive(hologramMat, new Color(0.2f, 1.2f, 1.8f));

            var root = GetOrCreateRoot(stage4, EnvironmentRootName);
            ClearChildren(root.transform);

            // 4-1 Elevator Lobby
            BuildCorridorSegment(root.transform, "Zone_4_1_Elevator", 
                new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_1_EntranceZ),
                new Vector3(Stage4CoreDimensions.CorridorWidth, 0.1f, 14f), 
                floorMat, wallMat, ceilingMat, metalMat);

            // 4-2 Penthouse Office
            BuildCorridorSegment(root.transform, "Zone_4_2_Office", 
                new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_2_OfficeZ),
                new Vector3(Stage4CoreDimensions.PenthouseWidth, 0.1f, 14f), 
                floorMat, wallMat, ceilingMat, metalMat);

            // 4-3 Balcony Night View
            BuildCorridorSegment(root.transform, "Zone_4_3_Balcony", 
                new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_3_BalconyZ),
                new Vector3(Stage4CoreDimensions.PenthouseWidth, 0.1f, 14f), 
                floorMat, wallMat, ceilingMat, metalMat, isBalcony: true, glass: glassMat);

            // 4-4 Security Corridor
            BuildCorridorSegment(root.transform, "Zone_4_4_Corridor", 
                new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_4_CorridorZ),
                new Vector3(Stage4CoreDimensions.CorridorWidth, 0.1f, 14f), 
                floorMat, wallMat, ceilingMat, metalMat);

            // 4-5 Mainframe Access
            BuildCorridorSegment(root.transform, "Zone_4_5_Access", 
                new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_5_MainframeAccessZ),
                new Vector3(Stage4CoreDimensions.CorridorWidth, 0.1f, 14f), 
                floorMat, wallMat, ceilingMat, metalMat);

            // 4-6 Boss P1 Room (Exo-suit Alex Chamber)
            BuildCorridorSegment(root.transform, "Zone_4_6_BossChamber", 
                new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_6_BossP1Z),
                new Vector3(Stage4CoreDimensions.PenthouseWidth, 0.1f, 14f), 
                floorMat, wallMat, ceilingMat, metalMat);

            // 4-7 Boss P2 Room (AI Mainframe Core)
            BuildCorridorSegment(root.transform, "Zone_4_7_CoreChamber", 
                new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_7_BossP2Z),
                new Vector3(Stage4CoreDimensions.PenthouseWidth, 0.1f, 18f), 
                floorMat, wallMat, ceilingMat, metalMat, isDigitalSpace: true, hologram: hologramMat);
        }

        public static void BuildArchitecture(Transform stage4)
        {
            var metalMat = EnsureMat("M_Core_Metal", new Color(0.25f, 0.27f, 0.3f));
            var warningMat = EnsureMat("M_Core_Warning", new Color(1f, 0.25f, 0.1f));
            var hologramMat = EnsureMat("M_Core_Hologram", new Color(0.1f, 0.8f, 1f, 0.4f));

            var root = GetOrCreateRoot(stage4, ArchitectureRootName);
            ClearChildren(root.transform);

            // Populate luxury furnitures in CEO Office (Zone 4_2)
            var officeRoot = new GameObject("CEO_Office_Furniture");
            officeRoot.transform.SetParent(root.transform, false);
            PlacePrefab(officeRoot.transform, "Lobby_Sofa/scene.gltf", PrefabFolder, 
                "CEO_Sofa", new Vector3(0f, 0f, -6f), Quaternion.Euler(0f, 180f, 0f), 1.2f);
            PlacePrefab(officeRoot.transform, "Noguchi_Table/Noguchi_Table/scene.gltf", PrefabFolder, 
                "CEO_Table_A", new Vector3(-2f, 0f, -5f), Quaternion.identity, 1f);
            PlacePrefab(officeRoot.transform, "Modern_Table/Modern_Table/scene.gltf", PrefabFolder, 
                "CEO_Table_B", new Vector3(2f, 0f, -5f), Quaternion.identity, 1.2f);

            // Mainframe access consoles (Zone 4_5)
            var accessRoot = new GameObject("Mainframe_Consoles");
            accessRoot.transform.SetParent(root.transform, false);
            PlacePrefab(accessRoot.transform, "Terminal/FuturisticTerminal/scene.gltf", Stage4PrefabFolder, 
                "Access_Terminal_L", new Vector3(-2.8f, 0f, 36f), Quaternion.Euler(0f, 90f, 0f), 1f);
            PlacePrefab(accessRoot.transform, "Terminal/FuturisticTerminal/scene.gltf", Stage4PrefabFolder, 
                "Access_Terminal_R", new Vector3(2.8f, 0f, 36f), Quaternion.Euler(0f, -90f, 0f), 1f);

            // Mainframe Core (Zone 4_7)
            var coreRoot = new GameObject("Mainframe_Core_Structure");
            coreRoot.transform.SetParent(root.transform, false);
            var coreModel = PlacePrefab(coreRoot.transform, "ReactorCore/SciFiReactorCore/scene.gltf", Stage4PrefabFolder, 
                "AI_Mainframe_Core", new Vector3(0f, 1.5f, 66f), Quaternion.identity, 1f);
            if (coreModel == null)
            {
                // Fallback core block if prefab loading failed
                var fallback = CreatePbCube("AI_Mainframe_Core_Fallback", coreRoot.transform, new Vector3(0f, 2f, 66f), new Vector3(3f, 3f, 3f));
                SetMat(fallback, hologramMat);
            }

            // Floating digital pillars around the core
            for (int i = 0; i < 4; i++)
            {
                var x = (i % 2 == 0 ? -4.5f : 4.5f);
                var z = 62f + (i / 2) * 8f;
                var pillar = CreatePbCube($"DigitalPillar_{i + 1}", coreRoot.transform, new Vector3(x, 2.2f, z), new Vector3(0.8f, 4f, 0.8f));
                SetMat(pillar, hologramMat);
            }
        }

        static void BuildLighting(Transform stage4)
        {
            var existing = stage4.Find("Core_Lighting");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var root = new GameObject("Core_Lighting");
            root.transform.SetParent(stage4, false);

            AddFluorescent(root.transform, "Light_Elevator", new Vector3(0f, 4f, Stage4CoreDimensions.Cut4_1_EntranceZ), dim: false);
            AddFluorescent(root.transform, "Light_Office", new Vector3(0f, 4f, Stage4CoreDimensions.Cut4_2_OfficeZ), dim: false);
            AddFluorescent(root.transform, "Light_Balcony", new Vector3(0f, 4.2f, Stage4CoreDimensions.Cut4_3_BalconyZ), dim: true);
            AddFluorescent(root.transform, "Light_Corridor", new Vector3(0f, 4f, Stage4CoreDimensions.Cut4_4_CorridorZ), dim: true, redAccent: true);
            AddFluorescent(root.transform, "Light_Access", new Vector3(0f, 4f, Stage4CoreDimensions.Cut4_5_MainframeAccessZ), dim: false);
            AddFluorescent(root.transform, "Light_BossP1", new Vector3(0f, 4f, Stage4CoreDimensions.Cut4_6_BossP1Z), dim: false, redAccent: true);
            AddFluorescent(root.transform, "Light_BossP2", new Vector3(0f, 4.2f, Stage4CoreDimensions.Cut4_7_BossP2Z), dim: false, cyanAccent: true);

            var volume = stage4.Find("Stage4_GlobalVolume");
            if (volume == null)
            {
                var volGo = new GameObject("Stage4_GlobalVolume");
                volGo.transform.SetParent(stage4, false);
                volGo.AddComponent<UnityEngine.Rendering.Volume>();
            }
        }

        static void BuildCorridorSegment(Transform parent, string name, Vector3 center, Vector3 floorScale,
            Material floor, Material wall, Material ceiling, Material metal,
            bool isBalcony = false, Material? glass = null, bool isDigitalSpace = false, Material? hologram = null)
        {
            var seg = new GameObject(name);
            seg.transform.SetParent(parent, false);
            seg.transform.position = center;

            var floorGo = CreatePbCube("Floor", seg.transform, center + Vector3.down * 0.05f, floorScale);
            SetMat(floorGo, floor);

            var halfW = floorScale.x * 0.5f;
            var h = Stage4CoreDimensions.CeilingHeight;

            if (isBalcony)
            {
                // Balcony has open sky on one side (R) and glass wall on another
                BuildWallSegment(seg.transform, "Wall_L", center + new Vector3(-halfW, h * 0.5f, 0f),
                    new Vector3(0.2f, h, floorScale.z), wall);
                
                // Glass railing on the right (R)
                var rail = CreatePbCube("GlassRailing", seg.transform, center + new Vector3(halfW, 0.6f, 0f),
                    new Vector3(0.1f, 1.2f, floorScale.z));
                SetMat(rail, glass);
            }
            else if (isDigitalSpace)
            {
                // Digital holographic space doesn't have regular walls, uses open frames
                var frameL = CreatePbCube("HoloFrame_L", seg.transform, center + new Vector3(-halfW, h * 0.5f, 0f),
                    new Vector3(0.3f, h, floorScale.z));
                SetMat(frameL, hologram);

                var frameR = CreatePbCube("HoloFrame_R", seg.transform, center + new Vector3(halfW, h * 0.5f, 0f),
                    new Vector3(0.3f, h, floorScale.z));
                SetMat(frameR, hologram);
            }
            else
            {
                // Standard walls and ceiling
                BuildWallSegment(seg.transform, "Wall_L", center + new Vector3(-halfW, h * 0.5f, 0f),
                    new Vector3(0.2f, h, floorScale.z), wall);
                BuildWallSegment(seg.transform, "Wall_R", center + new Vector3(halfW, h * 0.5f, 0f),
                    new Vector3(0.2f, h, floorScale.z), wall);
                BuildWallSegment(seg.transform, "Ceiling", center + new Vector3(0f, h, 0f),
                    new Vector3(floorScale.x, 0.15f, floorScale.z), ceiling);
            }
        }

        static void AddFluorescent(Transform parent, string name, Vector3 pos, bool dim,
            bool redAccent = false, bool cyanAccent = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = dim ? 9f : 16f;
            light.intensity = dim ? 0.7f : 1.5f;

            if (redAccent)
                light.color = new Color(1f, 0.25f, 0.15f);
            else if (cyanAccent)
                light.color = new Color(0.1f, 0.85f, 1f);
            else
                light.color = new Color(0.85f, 0.9f, 1f);
        }

        static GameObject? PlacePrefab(Transform parent, string relativePath, string folder, string name,
            Vector3 pos, Quaternion rot, float scale)
        {
            var path = $"{folder}/{relativePath}";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return null;

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            inst.name = name;
            inst.transform.position = pos;
            inst.transform.rotation = rot;
            inst.transform.localScale = Vector3.one * scale;
            return inst;
        }

        static void BuildWallSegment(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var wall = CreatePbCube(name, parent, pos, scale);
            SetMat(wall, mat);
        }

        static GameObject CreatePbCube(string name, Transform parent, Vector3 worldPos, Vector3 scale)
        {
            var pb = ShapeGenerator.CreateShape(ShapeType.Cube);
            var go = pb.gameObject;
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            return go;
        }

        static GameObject GetOrCreateRoot(Transform stage4, string name)
        {
            var existing = stage4.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(stage4, false);
            go.transform.localPosition = Vector3.zero;
            return go;
        }

        static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        static Material EnsureMat(string name, Color color)
        {
            var path = $"{MatFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
                return mat;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void SetEmissive(Material mat, Color emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissive);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        static void SetMat(GameObject go, Material? mat)
        {
            if (mat == null) return;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
        }
    }
}
#endif
