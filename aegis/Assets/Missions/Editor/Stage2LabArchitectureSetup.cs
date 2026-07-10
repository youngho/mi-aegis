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
    /// cut_timeline Stage2 + stage2_lab.md 기준 연구실 내부 구조물.
    /// Sketchfab: Lab_SciFiCorridor, Lab_BioChamber, Lab_ServerRack.
    /// </summary>
    public static class Stage2LabArchitectureSetup
    {
        const string Stage2ScenePath = "Assets/Scenes/Stages/Stage2_Lab.unity";
        const string ArchitectureRootName = "Architecture_Stage2_Lab";
        const string EnvironmentRootName = "Environment_Stage2_Lab";
        const string PrefabFolder = "Assets/Prefabs/Stage2_Lab";
        const string MatFolder = "Assets/Materials";

        [MenuItem("Aegis/Build Stage2 Lab Architecture")]
        public static void BuildFromMenu()
        {
            if (!System.IO.File.Exists(Stage2ScenePath))
            {
                Debug.LogError($"[Stage2LabArchitectureSetup] Missing {Stage2ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage2ScenePath, OpenSceneMode.Single);
            var stage2 = GameObject.Find("Stage2_Lab");
            if (stage2 == null)
            {
                Debug.LogError("[Stage2LabArchitectureSetup] Stage2_Lab root not found.");
                return;
            }

            PrepareStageRoot(stage2);
            BuildEnvironment(stage2.transform);
            BuildArchitecture(stage2.transform);
            BuildLighting(stage2.transform);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[Stage2LabArchitectureSetup] Lab corridors, incubation hall, server room, and props built.");
        }

        public static void BuildAll(Transform stage2)
        {
            PrepareStageRoot(stage2.gameObject);
            BuildEnvironment(stage2);
            BuildArchitecture(stage2);
            BuildLighting(stage2);
        }

        static void PrepareStageRoot(GameObject stage2)
        {
            // 레거시 플레이스홀더 제거
            var boss = GameObject.Find("Stage2_Lab_boss");
            if (boss != null)
                Object.DestroyImmediate(boss);

            var mesh = stage2.GetComponent<MeshRenderer>();
            if (mesh != null) Object.DestroyImmediate(mesh);
            var mf = stage2.GetComponent<MeshFilter>();
            if (mf != null) Object.DestroyImmediate(mf);
            var col = stage2.GetComponent<BoxCollider>();
            if (col != null) Object.DestroyImmediate(col);

            if (stage2.GetComponent<StageRoot>() == null)
                stage2.AddComponent<StageRoot>();
        }

        public static void BuildEnvironment(Transform stage2)
        {
            var floor = EnsureMat("M_Lab_Floor", new Color(0.12f, 0.14f, 0.16f));
            var wall = EnsureMat("M_Lab_Wall", new Color(0.18f, 0.2f, 0.22f));
            var ceiling = EnsureMat("M_Lab_Ceiling", new Color(0.08f, 0.09f, 0.11f));
            var metal = EnsureMat("M_Lab_Metal", new Color(0.25f, 0.28f, 0.32f));
            var glass = EnsureMat("M_Lab_Glass", new Color(0.3f, 0.85f, 0.75f, 0.35f));
            var emissive = EnsureMat("M_Lab_EmeraldFluid", new Color(0.1f, 0.95f, 0.55f));
            SetEmissive(emissive, new Color(0.2f, 1.8f, 0.9f));

            var root = GetOrCreateRoot(stage2, EnvironmentRootName);
            ClearChildren(root.transform);

            BuildCorridorSegment(root.transform, "Zone_2_1_DarkCorridor", new Vector3(0f, 0f, -14f),
                new Vector3(Stage2LabDimensions.CorridorWidth, 0.1f, 14f), floor, wall, ceiling, metal, dim: true);
            BuildCorridorSegment(root.transform, "Zone_2_2_IncubationHall", new Vector3(0f, 0f, -4f),
                new Vector3(Stage2LabDimensions.LabHallWidth, 0.1f, 14f), floor, wall, ceiling, metal, dim: true);
            BuildCorridorSegment(root.transform, "Zone_2_3_LabZone", new Vector3(0f, 0f, 8f),
                new Vector3(Stage2LabDimensions.LabHallWidth, 0.1f, 12f), floor, wall, ceiling, metal, dim: false);
            BuildCorridorSegment(root.transform, "Zone_2_4_Isolated", new Vector3(0f, 0f, 18f),
                new Vector3(Stage2LabDimensions.CorridorWidth, 0.1f, 10f), floor, wall, ceiling, metal, dim: true);
            BuildCorridorSegment(root.transform, "Zone_2_5_DataStorage", new Vector3(0f, 0f, 28f),
                new Vector3(Stage2LabDimensions.LabHallWidth, 0.1f, 12f), floor, wall, ceiling, metal, dim: false);
            BuildCorridorSegment(root.transform, "Zone_2_6_Shadow", new Vector3(0f, 0f, 38f),
                new Vector3(Stage2LabDimensions.CorridorWidth, 0.1f, 10f), floor, wall, ceiling, metal, dim: true);
            BuildCorridorSegment(root.transform, "Zone_2_7_BossArena", new Vector3(0f, 0f, 46f),
                new Vector3(Stage2LabDimensions.LabHallWidth, 0.1f, 10f), floor, wall, ceiling, metal, dim: false);

            // 깨진 배양 탱크 — 에메랄드 액체
            BuildFluidSpill(root.transform, "FluidSpill_Incubation", new Vector3(-2f, 0.02f, -2f), emissive);
            BuildFluidSpill(root.transform, "FluidSpill_Boss", new Vector3(1.5f, 0.02f, 45f), emissive);

            // 통로 봉쇄 (연출 4:30)
            BuildBlockade(root.transform, "Blockade_Alarm", new Vector3(0f, 0f, 12f), metal, glass);

            PlacePrefab(root.transform, "Lab_SciFiCorridor/scene.gltf", "Corridor_Visual_A",
                new Vector3(0f, 0f, -16f), Quaternion.identity, 1.2f);
            PlacePrefab(root.transform, "Lab_SciFiCorridor/scene.gltf", "Corridor_Visual_B",
                new Vector3(0f, 0f, 16f), Quaternion.Euler(0f, 180f, 0f), 1.2f);
        }

        public static void BuildArchitecture(Transform stage2)
        {
            var metal = EnsureMat("M_Lab_Metal", new Color(0.25f, 0.28f, 0.32f));
            var glass = EnsureMat("M_Lab_Glass", new Color(0.3f, 0.85f, 0.75f, 0.35f));

            var root = GetOrCreateRoot(stage2, ArchitectureRootName);
            ClearChildren(root.transform);

            BuildIncubationHall(root.transform, glass, metal);
            BuildDataStorage(root.transform, metal);
            BuildBossArena(root.transform, metal, glass);
        }

        static void BuildIncubationHall(Transform parent, Material glass, Material metal)
        {
            var hall = new GameObject("IncubationHall");
            hall.transform.SetParent(parent, false);

            var positions = new[]
            {
                new Vector3(-5f, 0f, -6f),
                new Vector3(-5f, 0f, -2f),
                new Vector3(5f, 0f, -4f),
                new Vector3(5f, 0f, 0f),
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                var tank = PlacePrefab(hall.transform, "Lab_BioChamber/scene.gltf",
                    $"BioChamber_{i + 1}", pos, Quaternion.Euler(0f, i % 2 == 0 ? 90f : -90f, 0f), 0.8f);
                if (tank == null)
                {
                    var fallback = CreatePbCube($"BioChamber_{i + 1}", hall.transform,
                        pos + Vector3.up * Stage2LabDimensions.IncubationTankCenterY,
                        new Vector3(1.2f, 2.8f, 1.2f));
                    SetMat(fallback, glass);
                }
            }

            // 깨진 탱크 잔해
            var shard = CreatePbCube("BrokenTankShard", hall.transform, new Vector3(-3f, 0.6f, -1f),
                new Vector3(0.8f, 1.2f, 0.15f));
            shard.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            SetMat(shard, glass);
        }

        static void BuildDataStorage(Transform parent, Material metal)
        {
            var storage = new GameObject("DataStorage");
            storage.transform.SetParent(parent, false);

            var rackPositions = new[]
            {
                new Vector3(-5f, 0f, 26f),
                new Vector3(-2.5f, 0f, 28f),
                new Vector3(0f, 0f, 30f),
                new Vector3(2.5f, 0f, 28f),
                new Vector3(5f, 0f, 26f),
                new Vector3(-3.5f, 0f, 31f),
                new Vector3(3.5f, 0f, 31f),
            };

            for (var i = 0; i < rackPositions.Length; i++)
            {
                var pos = rackPositions[i];
                var rack = PlacePrefab(storage.transform, "Lab_ServerRack/scene.gltf",
                    $"ServerRack_{i + 1}", pos, Quaternion.Euler(0f, 180f, 0f), 1.0f);
                if (rack == null)
                {
                    var fallback = CreatePbCube($"ServerRack_{i + 1}", storage.transform,
                        pos + Vector3.up * Stage2LabDimensions.ServerRackCenterY,
                        new Vector3(0.6f, 2.2f, 1.0f));
                    SetMat(fallback, metal);
                }
            }
        }

        static void BuildBossArena(Transform parent, Material metal, Material glass)
        {
            var arena = new GameObject("BossArena_RX7");
            arena.transform.SetParent(parent, false);

            // RX-7 강하 지점 마커 + 유리 파편
            var dropPoint = CreatePbCube("RX7_DropPoint", arena.transform,
                new Vector3(0f, Stage2LabDimensions.CeilingHeight - 0.3f, 46f),
                new Vector3(2f, 0.1f, 2f));
            SetMat(dropPoint, metal);

            foreach (var (x, z, tilt) in new[] { (-2f, 45.5f, -15f), (1.5f, 46f, 8f), (0f, 47f, 22f) })
            {
                var shard = CreatePbCube("GlassShard", arena.transform, new Vector3(x, 0.8f, z),
                    new Vector3(0.9f, 1.4f, 0.05f));
                shard.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
                SetMat(shard, glass);
            }

            PlacePrefab(arena.transform, "Lab_SciFiSoldier/scene.gltf", "RX7_Boss_Visual",
                new Vector3(0f, 0f, 48f), Quaternion.Euler(0f, 180f, 0f), 0.015f);
        }

        static void BuildLighting(Transform stage2)
        {
            var existing = stage2.Find("Lab_Lighting");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var root = new GameObject("Lab_Lighting");
            root.transform.SetParent(stage2, false);

            AddFluorescent(root.transform, "Light_Corridor_A", new Vector3(0f, 3.8f, -14f), dim: true);
            AddFluorescent(root.transform, "Light_Incubation", new Vector3(0f, 3.8f, -4f), dim: true);
            AddFluorescent(root.transform, "Light_LabZone", new Vector3(0f, 3.8f, 8f), dim: false);
            AddFluorescent(root.transform, "Light_Isolated", new Vector3(0f, 3.8f, 18f), dim: true);
            AddFluorescent(root.transform, "Light_DataStorage", new Vector3(0f, 3.8f, 28f), dim: false);
            AddFluorescent(root.transform, "Light_Shadow", new Vector3(0f, 3.8f, 38f), dim: true);
            AddFluorescent(root.transform, "Light_Boss", new Vector3(0f, 3.8f, 46f), dim: false, redAccent: true);

            var volume = stage2.Find("Stage2_GlobalVolume");
            if (volume == null)
            {
                var volGo = new GameObject("Stage2_GlobalVolume");
                volGo.transform.SetParent(stage2, false);
                volGo.AddComponent<UnityEngine.Rendering.Volume>();
            }
        }

        static void BuildCorridorSegment(Transform parent, string name, Vector3 center, Vector3 floorScale,
            Material floor, Material wall, Material ceiling, Material metal, bool dim)
        {
            var seg = new GameObject(name);
            seg.transform.SetParent(parent, false);
            seg.transform.position = center;

            var floorGo = CreatePbCube("Floor", seg.transform, center + Vector3.down * 0.05f, floorScale);
            SetMat(floorGo, floor);

            var halfW = floorScale.x * 0.5f;
            var halfD = floorScale.z * 0.5f;
            var h = Stage2LabDimensions.CeilingHeight;

            BuildWallSegment(seg.transform, "Wall_L", center + new Vector3(-halfW, h * 0.5f, 0f),
                new Vector3(0.2f, h, floorScale.z), wall);
            BuildWallSegment(seg.transform, "Wall_R", center + new Vector3(halfW, h * 0.5f, 0f),
                new Vector3(0.2f, h, floorScale.z), wall);
            BuildWallSegment(seg.transform, "Ceiling", center + new Vector3(0f, h, 0f),
                new Vector3(floorScale.x, 0.15f, floorScale.z), ceiling);

            if (dim)
            {
                var pipe = CreatePbCube("Pipe", seg.transform, center + new Vector3(halfW - 0.3f, 1.5f, 0f),
                    new Vector3(0.08f, 0.08f, floorScale.z * 0.8f));
                SetMat(pipe, metal);
            }
        }

        static void BuildBlockade(Transform parent, string name, Vector3 pos, Material metal, Material glass)
        {
            var block = new GameObject(name);
            block.transform.SetParent(parent, false);
            block.transform.position = pos;

            var gateL = CreatePbCube("Gate_L", block.transform, pos + new Vector3(-1.5f, 1.5f, 0f),
                new Vector3(0.15f, 3f, 2f));
            var gateR = CreatePbCube("Gate_R", block.transform, pos + new Vector3(1.5f, 1.5f, 0f),
                new Vector3(0.15f, 3f, 2f));
            SetMat(gateL, metal);
            SetMat(gateR, metal);

            var warning = CreatePbCube("WarningLight", block.transform, pos + new Vector3(0f, 2.8f, 0.5f),
                new Vector3(0.3f, 0.15f, 0.1f));
            SetMat(warning, glass);
        }

        static void BuildFluidSpill(Transform parent, string name, Vector3 pos, Material emissive)
        {
            var spill = CreatePbCube(name, parent, pos, new Vector3(2.5f, 0.04f, 1.8f));
            SetMat(spill, emissive);
        }

        static void AddFluorescent(Transform parent, string name, Vector3 pos, bool dim, bool redAccent = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = dim ? 8f : 14f;
            light.intensity = dim ? 0.6f : 1.4f;
            light.color = redAccent ? new Color(1f, 0.25f, 0.2f) : new Color(0.75f, 0.85f, 1f);
        }

        static GameObject? PlacePrefab(Transform parent, string relativePath, string name,
            Vector3 pos, Quaternion rot, float scale)
        {
            var path = $"{PrefabFolder}/{relativePath}";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[Stage2LabArchitectureSetup] Missing prefab: {path}");
                return null;
            }

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

        static GameObject GetOrCreateRoot(Transform stage2, string name)
        {
            var existing = stage2.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(stage2, false);
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
            if (color.a < 1f)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
            }

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
