#if UNITY_EDITOR
#nullable enable
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using PinkSoft.Aegis.Missions;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>docs/stages/stage3_datacenter.md + docs/design/cut_timeline.md 기준 Stage3 컷/스폰/카메라 배치.</summary>
    public static class Stage3SceneSetup
    {
        const string Stage3Path = "Stage3_Datacenter";
        const string CutsRootName = "Stage3_Cuts";
        const string CameraPathName = "CameraPath_Stage3";
        const string CyborgPrefabPath = "Assets/Prefabs/Stage2_Lab/Lab_SciFiSoldier/scene.gltf";
        const string GruntPrefabPath = "Assets/Prefabs/Soldier_Grunt/scene.gltf";
        const string DronePrefabPath = "Assets/Prefabs/Stage3_Datacenter/DC_SurveillanceDrone_1/scene.gltf";

        static readonly Color GruntColor = new(0.85f, 0.25f, 0.2f);
        static readonly Color CyborgColor = new(0.15f, 0.75f, 0.85f);
        static readonly Color ShieldColor = new(0.2f, 0.45f, 0.9f);
        static readonly Color SniperColor = new(0.95f, 0.75f, 0.15f);
        static readonly Color DroneColor = new(0.2f, 0.85f, 0.9f);
        static readonly Color BossColor = new(0.65f, 0.15f, 0.95f);

        [MenuItem("Aegis/Setup Stage3 Cuts In Active Scene")]
        public static void SetupActiveScene()
        {
            var stage3 = GameObject.Find(Stage3Path);
            if (stage3 == null)
            {
                Debug.LogError($"[Stage3SceneSetup] '{Stage3Path}' not found in active scene.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(stage3, "Setup Stage3 Cuts");

            Stage3DatacenterArchitectureSetup.BuildAll(stage3.transform);

            var cutsRoot = GetOrCreate(stage3.transform, CutsRootName);
            ClearChildren(cutsRoot.transform);

            BuildCut3_1(cutsRoot.transform);
            BuildCut3_2(cutsRoot.transform);
            BuildCut3_3(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_3_PowerCut", new Vector3(0f, 2f, 22f),
                "4:20-4:40 전력 차단");
            BuildCut3_4(cutsRoot.transform);
            BuildCut3_5(cutsRoot.transform);
            BuildCut3_6(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_3_Overload", new Vector3(0f, 2f, 56f),
                "9:10-9:30 오버로드 활성화");
            BuildCut3_7(cutsRoot.transform);

            SetupCameras(stage3.transform);
            EnsureStageRoot(stage3);
            WireMissionReferences();

            EditorUtility.SetDirty(stage3);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[Stage3SceneSetup] Stage3 cuts, spawns, and cameras configured.");
        }

        static GameObject GetOrCreate(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            return go;
        }

        static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }

        static void BuildCut3_1(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_3_1_Entrance", new Vector3(0f, 0f, Stage3DatacenterDimensions.Cut3_1_EntranceZ),
                "3-1 서버실 입구 스파크 | 그런트×4");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-2.5f, 1f, -16f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(2.5f, 1f, -15f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-2f, 1f, -12f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(2f, 1f, -12f));

            CreateEnemy(cut.transform, "grunt_entrance_L_01", new Vector3(-2.5f, 1f, -16f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_entrance_R_01", new Vector3(2.5f, 1f, -15f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_entrance_L_02", new Vector3(-2f, 1f, -12f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_entrance_R_02", new Vector3(2f, 1f, -12f), GruntColor, EnemyKind.Grunt);
        }

        static void BuildCut3_2(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_3_2_Smoke", new Vector3(0f, 0f, Stage3DatacenterDimensions.Cut3_2_SmokeZ),
                "3-2 연기 통로 | 사이보그×5 드론×2");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-3f, 1f, -4f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(3f, 1f, -3f));
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, 1f, 0f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-2f, Stage3DatacenterDimensions.AirSpawnHeight, 2f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(2f, Stage3DatacenterDimensions.AirSpawnHeight, 2f));

            CreateEnemy(cut.transform, "cyborg_smoke_L_01", new Vector3(-3f, 1f, -4f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_smoke_R_01", new Vector3(3f, 1f, -3f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_smoke_C_02", new Vector3(0f, 1f, 0f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_smoke_L_03", new Vector3(-2.5f, 1f, 1f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_smoke_R_04", new Vector3(2.5f, 1f, 1.5f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "drone_smoke_L", new Vector3(-2f, Stage3DatacenterDimensions.AirSpawnHeight, 2f),
                DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "drone_smoke_R", new Vector3(2f, Stage3DatacenterDimensions.AirSpawnHeight, 2f),
                DroneColor, EnemyKind.Drone);
        }

        static void BuildCut3_3(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_3_3_SubServer", new Vector3(0f, 0f, Stage3DatacenterDimensions.Cut3_3_SubServerZ),
                "3-3 서브서버 파괴 | 방어 그런트×3 + 서버 4개");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4f, 1f, 8f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(4f, 1f, 8f));
            CreateSpawnSlot(cut.transform, "spawn_C_near", new Vector3(0f, 1f, 12f));

            CreateEnemy(cut.transform, "grunt_defend_L", new Vector3(-4f, 1f, 8f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_defend_R", new Vector3(4f, 1f, 8f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_defend_C", new Vector3(0f, 1f, 12f), GruntColor, EnemyKind.Grunt);

            CreateGimmickTarget(cut.transform, "subserver_target_1", new Vector3(-4f, 1.5f, 10f), new Color(0.9f, 0.5f, 0.1f));
            CreateGimmickTarget(cut.transform, "subserver_target_2", new Vector3(-1.5f, 1.5f, 10f), new Color(0.9f, 0.5f, 0.1f));
            CreateGimmickTarget(cut.transform, "subserver_target_3", new Vector3(1.5f, 1.5f, 10f), new Color(0.9f, 0.5f, 0.1f));
            CreateGimmickTarget(cut.transform, "subserver_target_4", new Vector3(4f, 1.5f, 10f), new Color(0.9f, 0.5f, 0.1f));
        }

        static void BuildCut3_4(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_3_4_PowerControl", new Vector3(0f, 0f, Stage3DatacenterDimensions.Cut3_4_PowerControlZ),
                "3-4 전력 제어소 | 저격×2(상) 그런트×4(하)");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-3f, 1f, 24f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(3f, 1f, 24f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-5f, Stage3DatacenterDimensions.CeilingSpawnHeight, 26f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(5f, Stage3DatacenterDimensions.CeilingSpawnHeight, 26f));

            CreateEnemy(cut.transform, "grunt_power_L_01", new Vector3(-3f, 1f, 24f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_power_R_01", new Vector3(3f, 1f, 24f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_power_L_02", new Vector3(-2f, 1f, 27f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_power_R_02", new Vector3(2f, 1f, 27f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "sniper_power_L", new Vector3(-5f, Stage3DatacenterDimensions.CeilingSpawnHeight, 26f),
                SniperColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "sniper_power_R", new Vector3(5f, Stage3DatacenterDimensions.CeilingSpawnHeight, 26f),
                SniperColor, EnemyKind.Cyborg);
        }

        static void BuildCut3_5(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_3_5_BombDefusal", new Vector3(0f, 0f, Stage3DatacenterDimensions.Cut3_5_BombDefusalZ),
                "3-5 폭탄 해체 | 경비 그런트×5 + 장치 3소");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-3f, 1f, 35f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(3f, 1f, 35f));
            CreateSpawnSlot(cut.transform, "spawn_C_near", new Vector3(0f, 1f, 38f));

            CreateEnemy(cut.transform, "grunt_guard_L_01", new Vector3(-3f, 1f, 35f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_guard_R_01", new Vector3(3f, 1f, 35f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_guard_L_02", new Vector3(-2f, 1f, 37f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_guard_R_02", new Vector3(2f, 1f, 37f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_guard_C", new Vector3(0f, 1f, 38f), GruntColor, EnemyKind.Grunt);

            CreateGimmickTarget(cut.transform, "bomb_device_red", new Vector3(-1.2f, 1.2f, 35.5f), new Color(0.9f, 0.15f, 0.1f));
            CreateGimmickTarget(cut.transform, "bomb_device_blue", new Vector3(0f, 1.2f, 35.5f), new Color(0.1f, 0.3f, 0.9f));
            CreateGimmickTarget(cut.transform, "bomb_device_yellow", new Vector3(1.2f, 1.2f, 35.5f), new Color(0.95f, 0.85f, 0.1f));
        }

        static void BuildCut3_6(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_3_6_Cooling", new Vector3(0f, 0f, Stage3DatacenterDimensions.Cut3_6_CoolingZ),
                "3-6 냉각 구역 증기 | 방패×3 드론×3");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-2.5f, 1f, 46f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(2.5f, 1f, 47f));
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, Stage3DatacenterDimensions.AirSpawnHeight, 50f));

            CreateEnemy(cut.transform, "shield_cooling_L", new Vector3(-2.5f, 1f, 46f), ShieldColor, EnemyKind.Shield,
                new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "shield_cooling_C", new Vector3(0f, 1f, 48f), ShieldColor, EnemyKind.Shield,
                new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "shield_cooling_R", new Vector3(2.5f, 1f, 47f), ShieldColor, EnemyKind.Shield,
                new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "drone_cooling_01", new Vector3(-2f, Stage3DatacenterDimensions.AirSpawnHeight, 49f),
                DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "drone_cooling_02", new Vector3(0f, Stage3DatacenterDimensions.AirSpawnHeight, 50f),
                DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "drone_cooling_03", new Vector3(2f, Stage3DatacenterDimensions.AirSpawnHeight, 49.5f),
                DroneColor, EnemyKind.Drone);
        }

        static void BuildCut3_7(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_3_7_Boss_Overload", new Vector3(0f, 0f, Stage3DatacenterDimensions.Cut3_7_BossZ),
                "3-7 오버로드 보스 | 실드 드론×4 + 본체");
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, Stage3DatacenterDimensions.BossHoverHeight, 60f));
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-3f, Stage3DatacenterDimensions.AirSpawnHeight, 59f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(3f, Stage3DatacenterDimensions.AirSpawnHeight, 59f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-4f, Stage3DatacenterDimensions.AirSpawnHeight, 61f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(4f, Stage3DatacenterDimensions.AirSpawnHeight, 61f));

            CreateEnemy(cut.transform, "boss_overload_body", new Vector3(0f, Stage3DatacenterDimensions.BossHoverHeight, 60f),
                BossColor, EnemyKind.Boss, droneScale: 0.018f);
            CreateEnemy(cut.transform, "shield_drone_L", new Vector3(-3f, Stage3DatacenterDimensions.AirSpawnHeight, 59f),
                DroneColor, EnemyKind.Drone, droneScale: 0.012f);
            CreateEnemy(cut.transform, "shield_drone_R", new Vector3(3f, Stage3DatacenterDimensions.AirSpawnHeight, 59f),
                DroneColor, EnemyKind.Drone, droneScale: 0.012f);
            CreateEnemy(cut.transform, "shield_drone_L2", new Vector3(-4f, Stage3DatacenterDimensions.AirSpawnHeight, 61f),
                DroneColor, EnemyKind.Drone, droneScale: 0.012f);
            CreateEnemy(cut.transform, "shield_drone_R2", new Vector3(4f, Stage3DatacenterDimensions.AirSpawnHeight, 61f),
                DroneColor, EnemyKind.Drone, droneScale: 0.012f);
        }

        static void BuildTransition(Transform parent, string name, Vector3 pos, string note)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
        }

        static GameObject CreateCut(Transform parent, string name, Vector3 anchor, string note)
        {
            var cut = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(cut, "Create " + name);
            cut.transform.SetParent(parent, false);
            cut.transform.position = anchor;

            var trigger = cut.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(18f, 5f, 14f);
            trigger.center = new Vector3(0f, 2f, 0f);

            return cut;
        }

        static void CreateSpawnSlot(Transform parent, string name, Vector3 worldPos)
        {
            var slot = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(slot, "Create " + name);
            slot.transform.SetParent(parent, false);
            slot.transform.position = worldPos;

            var gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gizmo.name = "Gizmo";
            gizmo.transform.SetParent(slot.transform, false);
            gizmo.transform.localScale = Vector3.one * 0.25f;
            var col = gizmo.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var r = gizmo.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = CreateMat(new Color(0.3f, 0.9f, 1f, 0.35f));
        }

        static void CreateGimmickTarget(Transform parent, string name, Vector3 worldPos, Color color)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(target, "Create " + name);
            target.name = name;
            target.transform.SetParent(parent, false);
            target.transform.position = worldPos;
            target.transform.localScale = new Vector3(0.5f, 0.8f, 0.3f);
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = CreateMat(color);
        }

        enum EnemyKind { Grunt, Cyborg, Shield, Drone, Boss }

        static void CreateEnemy(Transform parent, string name, Vector3 worldPos, Color color, EnemyKind kind,
            Vector3? scale = null, float? droneScale = null)
        {
            string? prefabPath = kind switch
            {
                EnemyKind.Cyborg or EnemyKind.Boss => kind == EnemyKind.Boss && droneScale.HasValue
                    ? DronePrefabPath
                    : CyborgPrefabPath,
                EnemyKind.Drone => DronePrefabPath,
                _ => GruntPrefabPath
            };

            GameObject? prefab = null;
            if (!string.IsNullOrEmpty(prefabPath))
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject enemy;
            if (prefab != null)
            {
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(enemy, "Create " + name);
                enemy.name = name;
                enemy.transform.SetParent(parent, false);
                enemy.transform.position = worldPos;

                if (kind == EnemyKind.Drone || (kind == EnemyKind.Boss && droneScale.HasValue))
                {
                    enemy.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one * (droneScale ?? 0.012f);
                }
                else if (kind == EnemyKind.Cyborg || kind == EnemyKind.Boss)
                {
                    enemy.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one * 0.009f;
                }
                else
                {
                    enemy.transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one * 0.009f;
                }
            }
            else
            {
                enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(enemy, "Create " + name);
                enemy.name = name;
                enemy.transform.SetParent(parent, false);
                enemy.transform.position = worldPos;
                enemy.transform.localScale = scale ?? new Vector3(0.6f, 1.2f, 0.4f);
                var renderer = enemy.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = CreateMat(color);
            }
        }

        static Material CreateMat(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
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
            mat.color = color;
            return mat;
        }

        static void SetupCameras(Transform stage3)
        {
            var cameraPath = stage3.Find(CameraPathName);
            if (cameraPath == null)
            {
                var pathGo = new GameObject(CameraPathName);
                Undo.RegisterCreatedObjectUndo(pathGo, "Create " + CameraPathName);
                pathGo.transform.SetParent(stage3, false);
                cameraPath = pathGo.transform;
            }

            var presets = new (string name, Vector3 pos, Vector3 lookAt, float fov)[]
            {
                ("Vcam_3_1_Entrance", new Vector3(0f, 1.6f, -20f), new Vector3(0f, 1.2f, -14f), 50f),
                ("Vcam_3_2_Smoke", new Vector3(-2f, 1.4f, -6f), new Vector3(0f, 1.2f, 0f), 48f),
                ("Vcam_3_3_SubServer", new Vector3(0f, 2f, 6f), new Vector3(0f, 1.5f, 12f), 42f),
                ("Vcam_3_4_PowerControl", new Vector3(0f, 2.4f, 20f), new Vector3(0f, 1.5f, 26f), 40f),
                ("Vcam_3_5_BombDefusal", new Vector3(0f, 1.5f, 32f), new Vector3(0f, 1.3f, 36f), 38f),
                ("Vcam_3_6_Cooling", new Vector3(3f, 1.3f, 44f), new Vector3(0f, 1.2f, 50f), 46f),
                ("Vcam_3_7_Boss", new Vector3(0f, 1.8f, 52f), new Vector3(0f, 4f, 60f), 36f),
            };

            for (var i = 0; i < presets.Length; i++)
            {
                var (name, pos, lookAt, fov) = presets[i];
                var child = cameraPath.Find(name);
                if (child == null)
                {
                    var go = new GameObject(name);
                    Undo.RegisterCreatedObjectUndo(go, "Create " + name);
                    go.transform.SetParent(cameraPath, false);
                    go.AddComponent<CinemachineCamera>();
                    child = go.transform;
                }

                child.localPosition = pos;
                child.rotation = Quaternion.LookRotation(lookAt - pos, Vector3.up);

                var vcam = child.GetComponent<CinemachineCamera>();
                if (vcam != null)
                {
                    var lens = vcam.Lens;
                    lens.FieldOfView = fov;
                    vcam.Lens = lens;
                    var priority = vcam.Priority;
                    priority.Value = i == 0 ? 15 : 10;
                    vcam.Priority = priority;
                }
            }

            EnsureStage3CameraController(cameraPath);
        }

        static void EnsureStage3CameraController(Transform cameraPath)
        {
            var ctrl = cameraPath.GetComponent<Stage3CameraController>();
            if (ctrl == null)
                ctrl = Undo.AddComponent<Stage3CameraController>(cameraPath.gameObject);

            var names = new[]
            {
                "Vcam_3_1_Entrance", "Vcam_3_2_Smoke", "Vcam_3_3_SubServer",
                "Vcam_3_4_PowerControl", "Vcam_3_5_BombDefusal", "Vcam_3_6_Cooling", "Vcam_3_7_Boss"
            };

            var so = new SerializedObject(ctrl);
            var camerasProp = so.FindProperty("virtualCameras");
            camerasProp.arraySize = names.Length;
            for (var i = 0; i < names.Length; i++)
            {
                CinemachineCamera? vcam = null;
                var child = cameraPath.Find(names[i]);
                if (child != null) vcam = child.GetComponent<CinemachineCamera>();
                camerasProp.GetArrayElementAtIndex(i).objectReferenceValue = vcam;
            }

            var mission = Object.FindAnyObjectByType<AegisMissionController>();
            if (mission != null)
                so.FindProperty("missionController").objectReferenceValue = mission;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureStageRoot(GameObject stage3)
        {
            if (stage3.GetComponent<StageRoot>() == null)
                Undo.AddComponent<StageRoot>(stage3);

            var stageManager = Object.FindAnyObjectByType<StageManager>();
            if (stageManager != null)
            {
                var root = stage3.GetComponent<StageRoot>();
                var so = new SerializedObject(root);
                so.FindProperty("stageManager").objectReferenceValue = stageManager;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void WireMissionReferences()
        {
            var mission = Object.FindAnyObjectByType<AegisMissionController>();
            var cameraCtrl = Object.FindAnyObjectByType<Stage3CameraController>();
            if (mission != null && cameraCtrl != null)
            {
                var so = new SerializedObject(cameraCtrl);
                so.FindProperty("missionController").objectReferenceValue = mission;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
