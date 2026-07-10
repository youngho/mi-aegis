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
    /// cut_timeline Stage3 + stage3_datacenter.md 기준 데이터센터 내부 구조물.
    /// Sketchfab: DC_MonitoringStation, DC_WallConsole, DC_SurveillanceDrone, DC_DataCenterRack.
    /// Stage2 재사용: Lab_ServerRack, Lab_SciFiCorridor.
    /// </summary>
    public static class Stage3DatacenterArchitectureSetup
    {
        const string Stage3ScenePath = "Assets/Scenes/Stages/Stage3_Datacenter.unity";
        const string ArchitectureRootName = "Architecture_Stage3_Datacenter";
        const string EnvironmentRootName = "Environment_Stage3_Datacenter";
        const string PrefabFolder = "Assets/Prefabs/Stage3_Datacenter";
        const string Stage2PrefabFolder = "Assets/Prefabs/Stage2_Lab";
        const string MatFolder = "Assets/Materials";

        [MenuItem("Aegis/Build Stage3 Datacenter Architecture")]
        public static void BuildFromMenu()
        {
            if (!System.IO.File.Exists(Stage3ScenePath))
            {
                Debug.LogError($"[Stage3DatacenterArchitectureSetup] Missing {Stage3ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage3ScenePath, OpenSceneMode.Single);
            var stage3 = GameObject.Find("Stage3_Datacenter");
            if (stage3 == null)
            {
                Debug.LogError("[Stage3DatacenterArchitectureSetup] Stage3_Datacenter root not found.");
                return;
            }

            PrepareStageRoot(stage3);
            BuildEnvironment(stage3.transform);
            BuildArchitecture(stage3.transform);
            BuildLighting(stage3.transform);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[Stage3DatacenterArchitectureSetup] Datacenter corridors, racks, control desk, and boss arena built.");
        }

        public static void BuildAll(Transform stage3)
        {
            PrepareStageRoot(stage3.gameObject);
            BuildEnvironment(stage3);
            BuildArchitecture(stage3);
            BuildLighting(stage3);
        }

        static void PrepareStageRoot(GameObject stage3)
        {
            var boss = GameObject.Find("Stage3_Datacenter_boss");
            if (boss != null)
                Object.DestroyImmediate(boss);

            foreach (var renderer in stage3.GetComponents<MeshRenderer>())
                Object.DestroyImmediate(renderer);
            foreach (var filter in stage3.GetComponents<MeshFilter>())
                Object.DestroyImmediate(filter);
            foreach (var collider in stage3.GetComponents<Collider>())
                Object.DestroyImmediate(collider);

            if (stage3.GetComponent<StageRoot>() == null)
                stage3.AddComponent<StageRoot>();
        }

        public static void BuildEnvironment(Transform stage3)
        {
            var floor = EnsureMat("M_DC_Floor", new Color(0.1f, 0.11f, 0.13f));
            var wall = EnsureMat("M_DC_Wall", new Color(0.14f, 0.15f, 0.18f));
            var ceiling = EnsureMat("M_DC_Ceiling", new Color(0.07f, 0.08f, 0.1f));
            var metal = EnsureMat("M_DC_Metal", new Color(0.22f, 0.24f, 0.28f));
            var emissive = EnsureMat("M_DC_Emissive", new Color(0.15f, 0.85f, 1f));
            var warning = EnsureMat("M_DC_Warning", new Color(1f, 0.25f, 0.1f));
            SetEmissive(emissive, new Color(0.3f, 1.5f, 2f));
            SetEmissive(warning, new Color(2f, 0.4f, 0.1f));

            var root = GetOrCreateRoot(stage3, EnvironmentRootName);
            ClearChildren(root.transform);

            BuildCorridorSegment(root.transform, "Zone_3_1_Entrance", new Vector3(0f, 0f, -14f),
                new Vector3(Stage3DatacenterDimensions.MazeCorridorWidth, 0.1f, 14f), floor, wall, ceiling, metal,
                sparkZone: true);
            BuildCorridorSegment(root.transform, "Zone_3_2_Smoke", new Vector3(0f, 0f, -2f),
                new Vector3(Stage3DatacenterDimensions.SteamZoneWidth, 0.1f, 14f), floor, wall, ceiling, metal,
                smoky: true);
            BuildCorridorSegment(root.transform, "Zone_3_3_SubServer", new Vector3(0f, 0f, 10f),
                new Vector3(Stage3DatacenterDimensions.ControlHallWidth, 0.1f, 14f), floor, wall, ceiling, metal);
            BuildCorridorSegment(root.transform, "Zone_3_4_PowerControl", new Vector3(0f, 0f, 24f),
                new Vector3(Stage3DatacenterDimensions.ControlHallWidth, 0.1f, 12f), floor, wall, ceiling, metal);
            BuildCorridorSegment(root.transform, "Zone_3_5_BombConsole", new Vector3(0f, 0f, 36f),
                new Vector3(Stage3DatacenterDimensions.ControlHallWidth, 0.1f, 12f), floor, wall, ceiling, metal);
            BuildCorridorSegment(root.transform, "Zone_3_6_Cooling", new Vector3(0f, 0f, 48f),
                new Vector3(Stage3DatacenterDimensions.SteamZoneWidth, 0.1f, 12f), floor, wall, ceiling, metal,
                smoky: true);
            BuildCorridorSegment(root.transform, "Zone_3_7_BossArena", new Vector3(0f, 0f, 60f),
                new Vector3(Stage3DatacenterDimensions.ControlHallWidth, 0.1f, 14f), floor, wall, ceiling, metal);

            BuildSparkFX(root.transform, "Spark_Entrance_A", new Vector3(-2f, 2.5f, -16f), emissive);
            BuildSparkFX(root.transform, "Spark_Entrance_B", new Vector3(2.5f, 1.8f, -15f), warning);
            BuildSteamVent(root.transform, "SteamVent_3_2_A", new Vector3(-3f, 0.5f, -4f), emissive);
            BuildSteamVent(root.transform, "SteamVent_3_2_B", new Vector3(3f, 0.5f, 0f), emissive);
            BuildSteamVent(root.transform, "SteamVent_3_6_A", new Vector3(-2f, 0.5f, 46f), emissive);
            BuildSteamVent(root.transform, "SteamVent_3_6_B", new Vector3(2f, 0.5f, 50f), emissive);

            BuildPowerCutoff(root.transform, "PowerCutoff_3_4", new Vector3(0f, 0f, 22f), metal, warning);

            PlacePrefab(root.transform, "Lab_SciFiCorridor/scene.gltf", Stage2PrefabFolder,
                "Corridor_Visual_A", new Vector3(0f, 0f, -16f), Quaternion.identity, 1.1f);
            PlacePrefab(root.transform, "Lab_SciFiCorridor/scene.gltf", Stage2PrefabFolder,
                "Corridor_Visual_B", new Vector3(0f, 0f, 14f), Quaternion.Euler(0f, 180f, 0f), 1.1f);
        }

        public static void BuildArchitecture(Transform stage3)
        {
            var metal = EnsureMat("M_DC_Metal", new Color(0.22f, 0.24f, 0.28f));
            var warning = EnsureMat("M_DC_Warning", new Color(1f, 0.25f, 0.1f));
            var bombRed = EnsureMat("M_DC_BombRed", new Color(0.9f, 0.15f, 0.1f));
            var bombBlue = EnsureMat("M_DC_BombBlue", new Color(0.1f, 0.3f, 0.9f));
            var bombYellow = EnsureMat("M_DC_BombYellow", new Color(0.95f, 0.85f, 0.1f));

            var root = GetOrCreateRoot(stage3, ArchitectureRootName);
            ClearChildren(root.transform);

            BuildServerMaze(root.transform, metal);
            BuildSubServerRoom(root.transform, metal, warning);
            BuildPowerControlRoom(root.transform, metal);
            BuildBombDefusalStation(root.transform, metal, bombRed, bombBlue, bombYellow);
            BuildCoolingZone(root.transform, metal);
            BuildBossArena(root.transform, metal, warning);
        }

        static void BuildServerMaze(Transform parent, Material metal)
        {
            var maze = new GameObject("ServerRackMaze");
            maze.transform.SetParent(parent, false);

            var positions = new[]
            {
                new Vector3(-3.5f, 0f, -16f), new Vector3(3.5f, 0f, -15f),
                new Vector3(-3f, 0f, -12f), new Vector3(3f, 0f, -11f),
                new Vector3(-3.5f, 0f, -8f), new Vector3(3.5f, 0f, -7f),
                new Vector3(-2f, 0f, -4f), new Vector3(2f, 0f, -3f),
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                var rack = PlacePrefab(maze.transform, "DC_DataCenterRack/scene.gltf", PrefabFolder,
                    $"Rack_Maze_{i + 1}", pos, Quaternion.Euler(0f, i % 2 == 0 ? 90f : -90f, 0f), 1f)
                    ?? PlacePrefab(maze.transform, "Lab_ServerRack/scene.gltf", Stage2PrefabFolder,
                        $"Rack_Maze_{i + 1}", pos, Quaternion.Euler(0f, 180f, 0f), 1f);
                if (rack == null)
                {
                    var fallback = CreatePbCube($"Rack_Maze_{i + 1}", maze.transform,
                        pos + Vector3.up * Stage3DatacenterDimensions.ServerRackCenterY,
                        new Vector3(0.6f, 2.2f, 1.0f));
                    SetMat(fallback, metal);
                }
            }
        }

        static void BuildSubServerRoom(Transform parent, Material metal, Material warning)
        {
            var room = new GameObject("SubServerRoom");
            room.transform.SetParent(parent, false);

            var destructiblePositions = new[]
            {
                new Vector3(-4f, 0f, 8f),
                new Vector3(-1.5f, 0f, 10f),
                new Vector3(1.5f, 0f, 10f),
                new Vector3(4f, 0f, 8f),
            };

            for (var i = 0; i < destructiblePositions.Length; i++)
            {
                var pos = destructiblePositions[i];
                var rack = PlacePrefab(room.transform, "Lab_ServerRack/scene.gltf", Stage2PrefabFolder,
                    $"SubServer_Target_{i + 1}", pos, Quaternion.Euler(0f, 180f, 0f), 1f);
                if (rack == null)
                {
                    var fallback = CreatePbCube($"SubServer_Target_{i + 1}", room.transform,
                        pos + Vector3.up * Stage3DatacenterDimensions.ServerRackCenterY,
                        new Vector3(0.6f, 2.2f, 1.0f));
                    SetMat(fallback, warning);
                }
            }

            for (var i = 0; i < 6; i++)
            {
                var x = (i % 2 == 0 ? -5.5f : 5.5f);
                var z = 6f + i * 1.2f;
                PlacePrefab(room.transform, "DC_DataCenterRack/scene.gltf", PrefabFolder,
                    $"Rack_Sub_{i + 1}", new Vector3(x, 0f, z), Quaternion.Euler(0f, x < 0 ? 90f : -90f, 0f), 1f);
            }
        }

        static void BuildPowerControlRoom(Transform parent, Material metal)
        {
            var room = new GameObject("PowerControlRoom");
            room.transform.SetParent(parent, false);

            PlacePrefab(room.transform, "DC_WallConsole/scene.gltf", PrefabFolder,
                "WallConsole_Main", new Vector3(0f, 1.5f, 26f), Quaternion.Euler(0f, 180f, 0f), 1.5f);
            PlacePrefab(room.transform, "DC_WallConsole/scene.gltf", PrefabFolder,
                "WallConsole_Left", new Vector3(-6f, 1.5f, 24f), Quaternion.Euler(0f, 90f, 0f), 1.2f);
            PlacePrefab(room.transform, "DC_WallConsole/scene.gltf", PrefabFolder,
                "WallConsole_Right", new Vector3(6f, 1.5f, 24f), Quaternion.Euler(0f, -90f, 0f), 1.2f);

            for (var i = 0; i < 4; i++)
            {
                var pos = new Vector3(-3f + i * 2f, 0f, 22f);
                var rack = PlacePrefab(room.transform, "Lab_ServerRack/scene.gltf", Stage2PrefabFolder,
                    $"Rack_Power_{i + 1}", pos, Quaternion.Euler(0f, 0f, 0f), 1f);
                if (rack == null)
                {
                    var fallback = CreatePbCube($"Rack_Power_{i + 1}", room.transform,
                        pos + Vector3.up * Stage3DatacenterDimensions.ServerRackCenterY,
                        new Vector3(0.6f, 2.2f, 1.0f));
                    SetMat(fallback, metal);
                }
            }
        }

        static void BuildBombDefusalStation(Transform parent, Material metal,
            Material bombRed, Material bombBlue, Material bombYellow)
        {
            var station = new GameObject("BombDefusalStation");
            station.transform.SetParent(parent, false);

            PlacePrefab(station.transform, "DC_MonitoringStation/scene.gltf", PrefabFolder,
                "ControlDesk_Center", new Vector3(0f, 0f, 36f), Quaternion.Euler(0f, 180f, 0f), 1.2f);

            var wireColors = new[] { ("BombWire_Red", bombRed, new Vector3(-1.2f, 1.2f, 35.5f)),
                ("BombWire_Blue", bombBlue, new Vector3(0f, 1.2f, 35.5f)),
                ("BombWire_Yellow", bombYellow, new Vector3(1.2f, 1.2f, 35.5f)) };

            foreach (var (name, mat, pos) in wireColors)
            {
                var terminal = CreatePbCube(name, station.transform, pos, new Vector3(0.25f, 0.35f, 0.15f));
                SetMat(terminal, mat);
            }

            var screen = CreatePbCube("BombLCD", station.transform, new Vector3(0f, 1.5f, 35.8f),
                new Vector3(0.8f, 0.5f, 0.05f));
            SetMat(screen, metal);

            for (var i = 0; i < 3; i++)
            {
                var pos = new Vector3(-4f + i * 4f, 0f, 38f);
                PlacePrefab(station.transform, "Lab_ServerRack/scene.gltf", Stage2PrefabFolder,
                    $"Rack_ConsoleCover_{i + 1}", pos, Quaternion.Euler(0f, 0f, 0f), 1f);
            }
        }

        static void BuildCoolingZone(Transform parent, Material metal)
        {
            var cooling = new GameObject("CoolingZone");
            cooling.transform.SetParent(parent, false);

            for (var i = 0; i < 6; i++)
            {
                var x = (i % 2 == 0 ? -4f : 4f);
                var z = 44f + (i / 2) * 2.5f;
                var pos = new Vector3(x, 0f, z);
                var rack = PlacePrefab(cooling.transform, "DC_DataCenterRack/scene.gltf", PrefabFolder,
                    $"Rack_Cooling_{i + 1}", pos, Quaternion.Euler(0f, x < 0 ? 90f : -90f, 0f), 1f)
                    ?? PlacePrefab(cooling.transform, "Lab_ServerRack/scene.gltf", Stage2PrefabFolder,
                        $"Rack_Cooling_{i + 1}", pos, Quaternion.Euler(0f, 180f, 0f), 1f);
                if (rack == null)
                {
                    var fallback = CreatePbCube($"Rack_Cooling_{i + 1}", cooling.transform,
                        pos + Vector3.up * Stage3DatacenterDimensions.ServerRackCenterY,
                        new Vector3(0.6f, 2.2f, 1.0f));
                    SetMat(fallback, metal);
                }
            }

            var pipe = CreatePbCube("CoolingPipe", cooling.transform, new Vector3(0f, 3.2f, 48f),
                new Vector3(0.3f, 0.3f, 8f));
            SetMat(pipe, metal);
        }

        static void BuildBossArena(Transform parent, Material metal, Material warning)
        {
            var arena = new GameObject("BossArena_Overload");
            arena.transform.SetParent(parent, false);

            var ringPositions = new[]
            {
                new Vector3(-5f, 0f, 58f), new Vector3(5f, 0f, 58f),
                new Vector3(-5f, 0f, 62f), new Vector3(5f, 0f, 62f),
                new Vector3(-3.5f, 0f, 56f), new Vector3(3.5f, 0f, 56f),
                new Vector3(-3.5f, 0f, 64f), new Vector3(3.5f, 0f, 64f),
            };

            for (var i = 0; i < ringPositions.Length; i++)
            {
                var pos = ringPositions[i];
                var rack = PlacePrefab(arena.transform, "Lab_ServerRack/scene.gltf", Stage2PrefabFolder,
                    $"Rack_BossRing_{i + 1}", pos, Quaternion.Euler(0f, 180f, 0f), 1.1f);
                if (rack == null)
                {
                    var fallback = CreatePbCube($"Rack_BossRing_{i + 1}", arena.transform,
                        pos + Vector3.up * Stage3DatacenterDimensions.ServerRackCenterY,
                        new Vector3(0.6f, 2.2f, 1.0f));
                    SetMat(fallback, metal);
                }
            }

            var coverL = CreatePbCube("CoverRack_L", arena.transform,
                new Vector3(-4f, Stage3DatacenterDimensions.ServerRackCenterY, 60f),
                new Vector3(0.8f, 2.4f, 1.2f));
            var coverR = CreatePbCube("CoverRack_R", arena.transform,
                new Vector3(4f, Stage3DatacenterDimensions.ServerRackCenterY, 60f),
                new Vector3(0.8f, 2.4f, 1.2f));
            SetMat(coverL, warning);
            SetMat(coverR, warning);

            PlacePrefab(arena.transform, "DC_SurveillanceDrone_1/scene.gltf", PrefabFolder,
                "Boss_Overload_Drone", new Vector3(0f, Stage3DatacenterDimensions.BossHoverHeight, 60f),
                Quaternion.Euler(0f, 180f, 0f), 0.015f);
        }

        static void BuildLighting(Transform stage3)
        {
            var existing = stage3.Find("DC_Lighting");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var root = new GameObject("DC_Lighting");
            root.transform.SetParent(stage3, false);

            AddFluorescent(root.transform, "Light_Entrance", new Vector3(0f, 4f, -14f), dim: true, flicker: true);
            AddFluorescent(root.transform, "Light_Smoke", new Vector3(0f, 4f, -2f), dim: true);
            AddFluorescent(root.transform, "Light_SubServer", new Vector3(0f, 4f, 10f), dim: false);
            AddFluorescent(root.transform, "Light_Power", new Vector3(0f, 4f, 24f), dim: false);
            AddFluorescent(root.transform, "Light_Console", new Vector3(0f, 4f, 36f), dim: false, redAccent: true);
            AddFluorescent(root.transform, "Light_Cooling", new Vector3(0f, 4f, 48f), dim: true);
            AddFluorescent(root.transform, "Light_Boss", new Vector3(0f, 4f, 60f), dim: false, redAccent: true);

            var volume = stage3.Find("Stage3_GlobalVolume");
            if (volume == null)
            {
                var volGo = new GameObject("Stage3_GlobalVolume");
                volGo.transform.SetParent(stage3, false);
                volGo.AddComponent<UnityEngine.Rendering.Volume>();
            }
        }

        static void BuildCorridorSegment(Transform parent, string name, Vector3 center, Vector3 floorScale,
            Material floor, Material wall, Material ceiling, Material metal,
            bool sparkZone = false, bool smoky = false)
        {
            var seg = new GameObject(name);
            seg.transform.SetParent(parent, false);
            seg.transform.position = center;

            var floorGo = CreatePbCube("Floor", seg.transform, center + Vector3.down * 0.05f, floorScale);
            SetMat(floorGo, floor);

            var halfW = floorScale.x * 0.5f;
            var h = Stage3DatacenterDimensions.CeilingHeight;

            BuildWallSegment(seg.transform, "Wall_L", center + new Vector3(-halfW, h * 0.5f, 0f),
                new Vector3(0.2f, h, floorScale.z), wall);
            BuildWallSegment(seg.transform, "Wall_R", center + new Vector3(halfW, h * 0.5f, 0f),
                new Vector3(0.2f, h, floorScale.z), wall);
            BuildWallSegment(seg.transform, "Ceiling", center + new Vector3(0f, h, 0f),
                new Vector3(floorScale.x, 0.15f, floorScale.z), ceiling);

            if (sparkZone || smoky)
            {
                var pipe = CreatePbCube("Pipe", seg.transform, center + new Vector3(halfW - 0.3f, 1.5f, 0f),
                    new Vector3(0.1f, 0.1f, floorScale.z * 0.7f));
                SetMat(pipe, metal);
            }
        }

        static void BuildSparkFX(Transform parent, string name, Vector3 pos, Material emissive)
        {
            var spark = CreatePbCube(name, parent, pos, new Vector3(0.15f, 0.15f, 0.15f));
            SetMat(spark, emissive);
        }

        static void BuildSteamVent(Transform parent, string name, Vector3 pos, Material emissive)
        {
            var vent = CreatePbCube(name, parent, pos, new Vector3(0.5f, 1.2f, 0.5f));
            SetMat(vent, emissive);
        }

        static void BuildPowerCutoff(Transform parent, string name, Vector3 pos, Material metal, Material warning)
        {
            var block = new GameObject(name);
            block.transform.SetParent(parent, false);
            block.transform.position = pos;

            var panel = CreatePbCube("Panel", block.transform, pos + new Vector3(0f, 2f, -0.5f),
                new Vector3(3f, 2f, 0.15f));
            SetMat(panel, metal);

            var light = CreatePbCube("WarningLight", block.transform, pos + new Vector3(0f, 3.2f, 0f),
                new Vector3(0.4f, 0.2f, 0.1f));
            SetMat(light, warning);
        }

        static void AddFluorescent(Transform parent, string name, Vector3 pos, bool dim,
            bool redAccent = false, bool flicker = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = dim ? 9f : 16f;
            light.intensity = flicker ? 0.8f : dim ? 0.7f : 1.5f;
            light.color = redAccent ? new Color(1f, 0.3f, 0.15f) : new Color(0.7f, 0.8f, 1f);
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

        static GameObject GetOrCreateRoot(Transform stage3, string name)
        {
            var existing = stage3.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(stage3, false);
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
