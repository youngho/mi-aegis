#if UNITY_EDITOR
#nullable enable
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using PinkSoft.Aegis.Missions;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>docs/stages/stage2_lab.md + docs/design/cut_timeline.md 기준 Stage2 컷/스폰/카메라 배치.</summary>
    public static class Stage2SceneSetup
    {
        const string Stage2Path = "Stage2_Lab";
        const string CutsRootName = "Stage2_Cuts";
        const string CameraPathName = "CameraPath_Stage2";
        const string CyborgPrefabPath = "Assets/Prefabs/Stage2_Lab/Lab_SciFiSoldier/scene.gltf";
        const string GruntPrefabPath = "Assets/Prefabs/Soldier_Grunt/scene.gltf";

        static readonly Color GruntColor = new(0.85f, 0.25f, 0.2f);
        static readonly Color CyborgColor = new(0.15f, 0.75f, 0.85f);
        static readonly Color ShieldColor = new(0.2f, 0.45f, 0.9f);
        static readonly Color SniperColor = new(0.95f, 0.75f, 0.15f);
        static readonly Color DroneColor = new(0.2f, 0.85f, 0.9f);
        static readonly Color BossColor = new(0.65f, 0.15f, 0.95f);

        [MenuItem("Aegis/Setup Stage2 Cuts In Active Scene")]
        public static void SetupActiveScene()
        {
            var stage2 = GameObject.Find(Stage2Path);
            if (stage2 == null)
            {
                Debug.LogError($"[Stage2SceneSetup] '{Stage2Path}' not found in active scene.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(stage2, "Setup Stage2 Cuts");

            Stage2LabArchitectureSetup.BuildAll(stage2.transform);

            var cutsRoot = GetOrCreate(stage2.transform, CutsRootName);
            ClearChildren(cutsRoot.transform);

            BuildCut2_1(cutsRoot.transform);
            BuildCut2_2(cutsRoot.transform);
            BuildCut2_3(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_2_AlarmBlockade", new Vector3(0f, 2f, 12f), "4:30-4:50 경보·통로 봉쇄");
            BuildCut2_4(cutsRoot.transform);
            BuildCut2_5(cutsRoot.transform);
            BuildCut2_6(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_2_RX7Entrance", new Vector3(0f, 2f, 44f), "9:20-9:40 RX-7 등장");
            BuildCut2_7(cutsRoot.transform);

            SetupCameras(stage2.transform);
            EnsureStageRoot(stage2);
            WireMissionReferences();

            EditorUtility.SetDirty(stage2);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[Stage2SceneSetup] Stage2 cuts, spawns, and cameras configured.");
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

        static void BuildCut2_1(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_2_1_Corridor", new Vector3(0f, 0f, Stage2LabDimensions.Cut2_1_CorridorZ),
                "2-1 어두운 복도 | 그런트×3 암전 기습");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-2.5f, 1f, -16f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(2.5f, 1f, -15f));
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, 1f, -12f));

            CreateEnemy(cut.transform, "grunt_ambush_L", new Vector3(-2.5f, 1f, -16f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_ambush_R", new Vector3(2.8f, 1f, -14.5f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_ambush_C", new Vector3(0.3f, 1f, -12.5f), GruntColor, EnemyKind.Grunt);
        }

        static void BuildCut2_2(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_2_2_Incubation", new Vector3(0f, 0f, Stage2LabDimensions.Cut2_2_IncubationZ),
                "2-2 배양 캡슐 실험실 | 사이보그 정찰병×4");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-5f, 1f, -5f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(5f, 1f, -3f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-4f, 1f, -1f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(4.5f, 1f, 1f));

            CreateEnemy(cut.transform, "cyborg_scout_L_01", new Vector3(-5f, 1f, -5f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_scout_R_01", new Vector3(5f, 1f, -3f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_scout_L_02", new Vector3(-4f, 1f, -1f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_scout_R_02", new Vector3(4.5f, 1f, 1f), CyborgColor, EnemyKind.Cyborg);
        }

        static void BuildCut2_3(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_2_3_LabZone", new Vector3(0f, 0f, Stage2LabDimensions.Cut2_3_LabZoneZ),
                "2-3 실험실 2구역 | 그런트×3 에너지 실드×2");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4f, 1f, 6f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(4f, 1f, 6f));
            CreateSpawnSlot(cut.transform, "spawn_C_near", new Vector3(0f, 1f, 9f));

            CreateEnemy(cut.transform, "grunt_lab_01", new Vector3(-3.5f, 1f, 7f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_lab_02", new Vector3(3.5f, 1f, 7.5f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_lab_03", new Vector3(0f, 1f, 10f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "shield_energy_L", new Vector3(-4f, 1f, 6f), ShieldColor, EnemyKind.Shield,
                new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "shield_energy_R", new Vector3(4f, 1f, 6f), ShieldColor, EnemyKind.Shield,
                new Vector3(0.9f, 1.3f, 0.5f));
        }

        static void BuildCut2_4(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_2_4_Isolated", new Vector3(0f, 0f, Stage2LabDimensions.Cut2_4_IsolatedZ),
                "2-4 고립 구역 | 사이보그 정찰병×5 웨이브2");
            var spawns = new[]
            {
                ("spawn_L_near", new Vector3(-2.5f, 1f, 16f)),
                ("spawn_R_near", new Vector3(2.5f, 1f, 17f)),
                ("spawn_C_far", new Vector3(0f, 1f, 20f)),
                ("spawn_L_far", new Vector3(-2f, Stage2LabDimensions.CeilingSpawnHeight, 19f)),
                ("spawn_R_far", new Vector3(2f, Stage2LabDimensions.CeilingSpawnHeight, 18f)),
            };
            foreach (var (name, pos) in spawns)
                CreateSpawnSlot(cut.transform, name, pos);

            CreateEnemy(cut.transform, "cyborg_iso_L_01", new Vector3(-2.5f, 1f, 16f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_iso_R_01", new Vector3(2.5f, 1f, 17f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_iso_C_02", new Vector3(0f, 1f, 20f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_iso_L_03", new Vector3(-2f, Stage2LabDimensions.CeilingSpawnHeight, 19f),
                CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_iso_R_04", new Vector3(2f, Stage2LabDimensions.CeilingSpawnHeight, 18f),
                CyborgColor, EnemyKind.Cyborg);
        }

        static void BuildCut2_5(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_2_5_DataStorage", new Vector3(0f, 0f, Stage2LabDimensions.Cut2_5_DataStorageZ),
                "2-5 데이터 보관실 | 그런트×4 저격 사이보그×2");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4f, 1f, 27f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(4f, 1f, 27f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-5f, Stage2LabDimensions.AirSpawnHeight, 30f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(5f, Stage2LabDimensions.AirSpawnHeight, 30f));

            CreateEnemy(cut.transform, "grunt_data_L_01", new Vector3(-4f, 1f, 27f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_data_R_01", new Vector3(4f, 1f, 27f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_data_L_02", new Vector3(-2f, 1f, 29f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "grunt_data_R_02", new Vector3(2f, 1f, 29.5f), GruntColor, EnemyKind.Grunt);
            CreateEnemy(cut.transform, "sniper_cyborg_L", new Vector3(-5f, Stage2LabDimensions.AirSpawnHeight, 30f),
                SniperColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "sniper_cyborg_R", new Vector3(5f, Stage2LabDimensions.AirSpawnHeight, 30f),
                SniperColor, EnemyKind.Cyborg);
        }

        static void BuildCut2_6(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_2_6_Shadow", new Vector3(0f, 0f, Stage2LabDimensions.Cut2_6_ShadowZ),
                "2-6 벽 그림자 | 방패×2 정찰병×3 RX-7 예고");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-2.5f, 1f, 36f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(2.5f, 1f, 37f));
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, 1f, 40f));

            CreateEnemy(cut.transform, "shield_shadow_L", new Vector3(-2.5f, 1f, 36f), ShieldColor, EnemyKind.Shield,
                new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "shield_shadow_R", new Vector3(2.5f, 1f, 37f), ShieldColor, EnemyKind.Shield,
                new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "cyborg_shadow_01", new Vector3(-1.5f, 1f, 39f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_shadow_02", new Vector3(1.5f, 1f, 39.5f), CyborgColor, EnemyKind.Cyborg);
            CreateEnemy(cut.transform, "cyborg_shadow_03", new Vector3(0f, 1f, 40f), CyborgColor, EnemyKind.Cyborg);
        }

        static void BuildCut2_7(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_2_7_Boss_RX7", new Vector3(0f, 0f, Stage2LabDimensions.Cut2_7_BossZ),
                "2-7 RX-7 보스 | 3페이즈 약점 3회");
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, 2.5f, 48f));
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-3f, 1.5f, 47f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(3f, 1.5f, 47f));

            CreateEnemy(cut.transform, "boss_rx7_body", new Vector3(0f, 1.8f, 48f), BossColor, EnemyKind.Boss,
                new Vector3(1.2f, 1.8f, 1.0f));
            CreateEnemy(cut.transform, "boss_rx7_arm_L", new Vector3(-1.2f, 1.5f, 47.5f), BossColor, EnemyKind.Boss,
                new Vector3(0.6f, 0.8f, 0.6f));
            CreateEnemy(cut.transform, "boss_rx7_arm_R", new Vector3(1.2f, 1.5f, 47.5f), BossColor, EnemyKind.Boss,
                new Vector3(0.6f, 0.8f, 0.6f));
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
            trigger.size = new Vector3(16f, 5f, 14f);
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

        enum EnemyKind { Grunt, Cyborg, Shield, Boss }

        static void CreateEnemy(Transform parent, string name, Vector3 worldPos, Color color, EnemyKind kind,
            Vector3? scale = null)
        {
            GameObject? prefab = null;
            string? prefabPath = kind switch
            {
                EnemyKind.Cyborg or EnemyKind.Boss => CyborgPrefabPath,
                _ => GruntPrefabPath
            };

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

                if (kind == EnemyKind.Cyborg || kind == EnemyKind.Boss)
                {
                    enemy.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one * (kind == EnemyKind.Boss ? 0.012f : 0.009f);
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

        static void SetupCameras(Transform stage2)
        {
            var cameraPath = stage2.Find(CameraPathName);
            if (cameraPath == null)
            {
                var pathGo = new GameObject(CameraPathName);
                Undo.RegisterCreatedObjectUndo(pathGo, "Create " + CameraPathName);
                pathGo.transform.SetParent(stage2, false);
                cameraPath = pathGo.transform;
            }

            var presets = new (string name, Vector3 pos, Vector3 lookAt, float fov)[]
            {
                ("Vcam_2_1_Corridor", new Vector3(0f, 1.5f, -20f), new Vector3(0f, 1.2f, -14f), 52f),
                ("Vcam_2_2_Incubation", new Vector3(-3f, 1.2f, -6f), new Vector3(0f, 1.5f, -2f), 45f),
                ("Vcam_2_3_LabZone", new Vector3(0f, 2.2f, 4f), new Vector3(0f, 1.5f, 10f), 42f),
                ("Vcam_2_4_Isolated", new Vector3(-2f, 1.8f, 14f), new Vector3(0f, 1.5f, 22f), 44f),
                ("Vcam_2_5_DataStorage", new Vector3(0f, 2.5f, 24f), new Vector3(0f, 1.8f, 30f), 40f),
                ("Vcam_2_6_Shadow", new Vector3(4f, 1.4f, 34f), new Vector3(0f, 1.3f, 40f), 48f),
                ("Vcam_2_7_Boss", new Vector3(0f, 1.2f, 40f), new Vector3(0f, 2f, 48f), 38f),
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

            EnsureStage2CameraController(cameraPath);
        }

        static void EnsureStage2CameraController(Transform cameraPath)
        {
            var ctrl = cameraPath.GetComponent<Stage2CameraController>();
            if (ctrl == null)
                ctrl = Undo.AddComponent<Stage2CameraController>(cameraPath.gameObject);

            var names = new[]
            {
                "Vcam_2_1_Corridor", "Vcam_2_2_Incubation", "Vcam_2_3_LabZone",
                "Vcam_2_4_Isolated", "Vcam_2_5_DataStorage", "Vcam_2_6_Shadow", "Vcam_2_7_Boss"
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

        static void EnsureStageRoot(GameObject stage2)
        {
            if (stage2.GetComponent<StageRoot>() == null)
                Undo.AddComponent<StageRoot>(stage2);

            var stageManager = Object.FindAnyObjectByType<StageManager>();
            if (stageManager != null)
            {
                var root = stage2.GetComponent<StageRoot>();
                var so = new SerializedObject(root);
                so.FindProperty("stageManager").objectReferenceValue = stageManager;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void WireMissionReferences()
        {
            var mission = Object.FindAnyObjectByType<AegisMissionController>();
            var cameraCtrl = Object.FindAnyObjectByType<Stage2CameraController>();
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
