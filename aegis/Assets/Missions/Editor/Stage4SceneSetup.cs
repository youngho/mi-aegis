#if UNITY_EDITOR
#nullable enable
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using PinkSoft.Aegis.Missions;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>docs/stages/stage4_core.md + docs/design/cut_timeline.md 기준 Stage4 컷/스폰/카메라 배치.</summary>
    public static class Stage4SceneSetup
    {
        const string Stage4Path = "Stage4_Core";
        const string CutsRootName = "Stage4_Cuts";
        const string CameraPathName = "CameraPath_Stage4";
        const string BossExosuitPrefabPath = "Assets/Prefabs/Stage4_Core/BossExosuit/BadassExosuit/scene.gltf";
        const string ReactorCorePrefabPath = "Assets/Prefabs/Stage4_Core/ReactorCore/SciFiReactorCore/scene.gltf";
        const string CyborgPrefabPath = "Assets/Prefabs/Stage2_Lab/Lab_SciFiSoldier/scene.gltf";
        const string GruntPrefabPath = "Assets/Prefabs/Soldier_Grunt/scene.gltf";
        const string DronePrefabPath = "Assets/Prefabs/Stage3_Datacenter/DC_SurveillanceDrone_1/scene.gltf";

        static readonly Color GruntColor = new(0.85f, 0.25f, 0.2f);
        static readonly Color CyborgColor = new(0.15f, 0.75f, 0.85f);
        static readonly Color ShieldColor = new(0.2f, 0.45f, 0.9f);
        static readonly Color SniperColor = new(0.95f, 0.75f, 0.15f);
        static readonly Color DroneColor = new(0.2f, 0.85f, 0.9f);
        static readonly Color BossColor = new(0.65f, 0.15f, 0.95f);

        [MenuItem("Aegis/Setup Stage4 Cuts In Active Scene")]
        public static void SetupActiveScene()
        {
            var stage4 = GameObject.Find(Stage4Path);
            if (stage4 == null)
            {
                Debug.LogError($"[Stage4SceneSetup] '{Stage4Path}' not found in active scene.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(stage4, "Setup Stage4 Cuts");

            Stage4CoreArchitectureSetup.BuildAll(stage4.transform);

            var cutsRoot = GetOrCreate(stage4.transform, CutsRootName);
            ClearChildren(cutsRoot.transform);

            BuildCut4_1(cutsRoot.transform);
            BuildCut4_2(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_4_BalconyMove", new Vector3(0f, 2f, 2f),
                "3:00-3:20 발코니 강제 이동");
            BuildCut4_3(cutsRoot.transform);
            BuildCut4_4(cutsRoot.transform);
            BuildCut4_5(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_4_BossMeet", new Vector3(0f, 2f, 44f),
                "7:50-8:10 수장 대치");
            BuildCut4_6(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_4_AegisRampage", new Vector3(0f, 2f, 58f),
                "12:30-13:00 이지스 폭주");
            BuildCut4_7(cutsRoot.transform);

            SetupCameras(stage4.transform);
            EnsureStageRoot(stage4);
            WireMissionReferences();

            EditorUtility.SetDirty(stage4);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[Stage4SceneSetup] Stage4 cuts, spawns, and cameras configured.");
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

        static void BuildCut4_1(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_4_1_Elevator", new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_1_EntranceZ),
                "4-1 엘리베이터 도착 | 엘리트 그런트×5");
            
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-3f, 1f, -16f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(3f, 1f, -15f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-2f, 1f, -12f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(2f, 1f, -12f));
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, 1f, -10f));

            CreateEnemy(cut.transform, "grunt_el_1", new Vector3(-3f, 1f, -16f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_el_2", new Vector3(3f, 1f, -15f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_el_3", new Vector3(-2f, 1f, -12f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_el_4", new Vector3(2f, 1f, -12f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_el_5", new Vector3(0f, 1f, -10f), GruntColor, EnemyKind.Grunt);
        }

        static void BuildCut4_2(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_4_2_Office", new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_2_OfficeZ),
                "4-2 회장실 내부 | 엘리트×4 저격×2");

            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4f, 1f, -4f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(4f, 1f, -4f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-2f, 1f, 0f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(2f, 1f, 0f));
            CreateSpawnSlot(cut.transform, "spawn_L_sniper", new Vector3(-5f, Stage4CoreDimensions.CeilingSpawnHeight, 2f));
            CreateSpawnSlot(cut.transform, "spawn_R_sniper", new Vector3(5f, Stage4CoreDimensions.CeilingSpawnHeight, 2f));

            CreateEnemy(cut.transform, "grunt_off_1", new Vector3(-4f, 1f, -4f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_off_2", new Vector3(4f, 1f, -4f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_off_3", new Vector3(-2f, 1f, 0f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_off_4", new Vector3(2f, 1f, 0f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "sniper_off_L", new Vector3(-5f, Stage4CoreDimensions.CeilingSpawnHeight, 2f), SniperColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "sniper_off_R", new Vector3(5f, Stage4CoreDimensions.CeilingSpawnHeight, 2f), SniperColor, EnemyKind.Cyborg);
        }

        static void BuildCut4_3(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_4_3_Balcony", new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_3_BalconyZ),
                "4-3 발코니 야경 | 엘리트×3 드론×4");

            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-3f, 1f, 6f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(3f, 1f, 6f));
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, 1f, 9f));
            CreateSpawnSlot(cut.transform, "spawn_drone_1", new Vector3(-4f, Stage4CoreDimensions.AirSpawnHeight, 8f));
            CreateSpawnSlot(cut.transform, "spawn_drone_2", new Vector3(-2f, Stage4CoreDimensions.AirSpawnHeight, 10f));
            CreateSpawnSlot(cut.transform, "spawn_drone_3", new Vector3(2f, Stage4CoreDimensions.AirSpawnHeight, 10f));
            CreateSpawnSlot(cut.transform, "spawn_drone_4", new Vector3(4f, Stage4CoreDimensions.AirSpawnHeight, 8f));

            CreateEnemy(cut.transform, "grunt_bal_1", new Vector3(-3f, 1f, 6f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_bal_2", new Vector3(3f, 1f, 6f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_bal_3", new Vector3(0f, 1f, 9f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "drone_bal_1", new Vector3(-4f, Stage4CoreDimensions.AirSpawnHeight, 8f), DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "drone_bal_2", new Vector3(-2f, Stage4CoreDimensions.AirSpawnHeight, 10f), DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "drone_bal_3", new Vector3(2f, Stage4CoreDimensions.AirSpawnHeight, 10f), DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "drone_bal_4", new Vector3(4f, Stage4CoreDimensions.AirSpawnHeight, 8f), DroneColor, EnemyKind.Drone);
        }

        static void BuildCut4_4(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_4_4_Corridor", new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_4_CorridorZ),
                "4-4 좁은 복도 | 엘리트×6");

            CreateSpawnSlot(cut.transform, "spawn_L1", new Vector3(-2f, 1f, 20f));
            CreateSpawnSlot(cut.transform, "spawn_R1", new Vector3(2f, 1f, 20f));
            CreateSpawnSlot(cut.transform, "spawn_C1", new Vector3(0f, 1f, 22f));
            CreateSpawnSlot(cut.transform, "spawn_L2", new Vector3(-2.5f, 1f, 24f));
            CreateSpawnSlot(cut.transform, "spawn_R2", new Vector3(2.5f, 1f, 24f));
            CreateSpawnSlot(cut.transform, "spawn_C2", new Vector3(0f, 1f, 26f));

            CreateEnemy(cut.transform, "grunt_cor_1", new Vector3(-2f, 1f, 20f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_cor_2", new Vector3(2f, 1f, 20f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_cor_3", new Vector3(0f, 1f, 22f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_cor_4", new Vector3(-2.5f, 1f, 24f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_cor_5", new Vector3(2.5f, 1f, 24f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_cor_6", new Vector3(0f, 1f, 26f), GruntColor, EnemyKind.Grunt);
        }

        static void BuildCut4_5(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_4_5_MainframeAccess", new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_5_MainframeAccessZ),
                "4-5 메인프레임 진입로 | 방패×3 엘리트×4");

            CreateSpawnSlot(cut.transform, "spawn_shield_L", new Vector3(-3f, 1f, 34f));
            CreateSpawnSlot(cut.transform, "spawn_shield_C", new Vector3(0f, 1f, 36f));
            CreateSpawnSlot(cut.transform, "spawn_shield_R", new Vector3(3f, 1f, 34f));
            CreateSpawnSlot(cut.transform, "spawn_grunt_1", new Vector3(-2.5f, 1f, 38f));
            CreateSpawnSlot(cut.transform, "spawn_grunt_2", new Vector3(-1f, 1f, 39f));
            CreateSpawnSlot(cut.transform, "spawn_grunt_3", new Vector3(1f, 1f, 39f));
            CreateSpawnSlot(cut.transform, "spawn_grunt_4", new Vector3(2.5f, 1f, 38f));

            CreateEnemy(cut.transform, "shield_acc_L", new Vector3(-3f, 1f, 34f), ShieldColor, EnemyKind.Shield, new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "shield_acc_C", new Vector3(0f, 1f, 36f), ShieldColor, EnemyKind.Shield, new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "shield_acc_R", new Vector3(3f, 1f, 34f), ShieldColor, EnemyKind.Shield, new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "grunt_acc_1", new Vector3(-2.5f, 1f, 38f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_acc_2", new Vector3(-1f, 1f, 39f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_acc_3", new Vector3(1f, 1f, 39f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_acc_4", new Vector3(2.5f, 1f, 38f), GruntColor, EnemyKind.Grunt);
        }

        static void BuildCut4_6(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_4_6_Boss_Alex", new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_6_BossP1Z),
                "4-6 수장 보스 P1 | 강화슈트 알렉스");

            CreateSpawnSlot(cut.transform, "spawn_boss_alex", new Vector3(0f, 1.8f, 50f));

            // Exosuit Alex Boss body
            CreateEnemy(cut.transform, "boss_alex_body", new Vector3(0f, 1.8f, 50f), BossColor, EnemyKind.Boss, exoScale: 1.2f);
        }

        static void BuildCut4_7(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_4_7_Boss_AegisCore", new Vector3(0f, 0f, Stage4CoreDimensions.Cut4_7_BossP2Z),
                "4-7 메인프레임 코어 P2 | 폭주 코어 이지스 + 실드 드론");

            CreateSpawnSlot(cut.transform, "spawn_boss_core", new Vector3(0f, 2f, 66f));
            CreateSpawnSlot(cut.transform, "spawn_drone_1", new Vector3(-3f, Stage4CoreDimensions.AirSpawnHeight, 64f));
            CreateSpawnSlot(cut.transform, "spawn_drone_2", new Vector3(3f, Stage4CoreDimensions.AirSpawnHeight, 64f));
            CreateSpawnSlot(cut.transform, "spawn_drone_3", new Vector3(-4f, Stage4CoreDimensions.AirSpawnHeight, 68f));
            CreateSpawnSlot(cut.transform, "spawn_drone_4", new Vector3(4f, Stage4CoreDimensions.AirSpawnHeight, 68f));

            // Central Core (imported SciFi Reactor Core)
            CreateEnemy(cut.transform, "boss_aegis_core", new Vector3(0f, 2f, 66f), BossColor, EnemyKind.BossCore, coreScale: 1f);

            // Shield drones
            CreateEnemy(cut.transform, "shield_drone_1", new Vector3(-3f, Stage4CoreDimensions.AirSpawnHeight, 64f), DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "shield_drone_2", new Vector3(3f, Stage4CoreDimensions.AirSpawnHeight, 64f), DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "shield_drone_3", new Vector3(-4f, Stage4CoreDimensions.AirSpawnHeight, 68f), DroneColor, EnemyKind.Drone);
            CreateEnemy(cut.transform, "shield_drone_4", new Vector3(4f, Stage4CoreDimensions.AirSpawnHeight, 68f), DroneColor, EnemyKind.Drone);
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

        enum EnemyKind { Grunt, Cyborg, Shield, Drone, Boss, BossCore }

        static void CreateEnemy(Transform parent, string name, Vector3 worldPos, Color color, EnemyKind kind,
            Vector3? scale = null, float? exoScale = null, float? coreScale = null)
        {
            string? prefabPath = kind switch
            {
                EnemyKind.Cyborg => CyborgPrefabPath,
                EnemyKind.Drone => DronePrefabPath,
                EnemyKind.Boss => BossExosuitPrefabPath,
                EnemyKind.BossCore => ReactorCorePrefabPath,
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

                if (kind == EnemyKind.Drone)
                {
                    enemy.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one * 0.012f;
                }
                else if (kind == EnemyKind.Boss)
                {
                    enemy.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one * (exoScale ?? 1f);
                }
                else if (kind == EnemyKind.BossCore)
                {
                    enemy.transform.localRotation = Quaternion.identity;
                    enemy.transform.localScale = Vector3.one * (coreScale ?? 1f);
                }
                else if (kind == EnemyKind.Cyborg)
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

        static void SetupCameras(Transform stage4)
        {
            var cameraPath = stage4.Find(CameraPathName);
            if (cameraPath == null)
            {
                var pathGo = new GameObject(CameraPathName);
                Undo.RegisterCreatedObjectUndo(pathGo, "Create " + CameraPathName);
                pathGo.transform.SetParent(stage4, false);
                cameraPath = pathGo.transform;
            }

            var presets = new (string name, Vector3 pos, Vector3 lookAt, float fov)[]
            {
                ("Vcam_4_1_Entrance", new Vector3(0f, 1.6f, -26f), new Vector3(0f, 1.2f, -20f), 50f),
                ("Vcam_4_2_Office", new Vector3(-2f, 1.4f, -12f), new Vector3(0f, 1.2f, -6f), 48f),
                ("Vcam_4_3_Balcony", new Vector3(0f, 2f, 2f), new Vector3(0f, 1.5f, 8f), 42f),
                ("Vcam_4_4_Corridor", new Vector3(0f, 2.4f, 16f), new Vector3(0f, 1.5f, 22f), 40f),
                ("Vcam_4_5_MainframeAccess", new Vector3(0f, 1.5f, 30f), new Vector3(0f, 1.3f, 36f), 38f),
                ("Vcam_4_6_Boss_Alex", new Vector3(0f, 1.8f, 44f), new Vector3(0f, 1.8f, 50f), 46f),
                ("Vcam_4_7_Boss_AegisCore", new Vector3(0f, 1.8f, 58f), new Vector3(0f, 2f, 66f), 36f),
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

            EnsureStage4CameraController(cameraPath);
        }

        static void EnsureStage4CameraController(Transform cameraPath)
        {
            var ctrl = cameraPath.GetComponent<Stage4CameraController>();
            if (ctrl == null)
                ctrl = Undo.AddComponent<Stage4CameraController>(cameraPath.gameObject);

            var names = new[]
            {
                "Vcam_4_1_Entrance", "Vcam_4_2_Office", "Vcam_4_3_Balcony",
                "Vcam_4_4_Corridor", "Vcam_4_5_MainframeAccess", "Vcam_4_6_Boss_Alex", "Vcam_4_7_Boss_AegisCore"
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

        static void EnsureStageRoot(GameObject stage4)
        {
            if (stage4.GetComponent<StageRoot>() == null)
                Undo.AddComponent<StageRoot>(stage4);

            var stageManager = Object.FindAnyObjectByType<StageManager>();
            if (stageManager != null)
            {
                var root = stage4.GetComponent<StageRoot>();
                var so = new SerializedObject(root);
                so.FindProperty("stageManager").objectReferenceValue = stageManager;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void WireMissionReferences()
        {
            var mission = Object.FindAnyObjectByType<AegisMissionController>();
            var cameraCtrl = Object.FindAnyObjectByType<Stage4CameraController>();
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
