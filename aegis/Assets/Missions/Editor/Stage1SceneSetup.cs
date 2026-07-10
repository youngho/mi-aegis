#if UNITY_EDITOR
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using PinkSoft.Aegis.Missions;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>docs/stages/stage1_lobby.md + docs/design/cut_timeline.md 기준 Stage1 컷/스폰/카메라 배치.</summary>
    public static class Stage1SceneSetup
    {
        const string Stage1Path = "Stage1_Lobby";
        const string CutsRootName = "Stage1_Cuts";

        static readonly Color GruntColor = new(0.85f, 0.25f, 0.2f);
        static readonly Color ShieldColor = new(0.2f, 0.45f, 0.9f);
        static readonly Color SniperColor = new(0.95f, 0.75f, 0.15f);
        static readonly Color DroneColor = new(0.2f, 0.85f, 0.9f);
        static readonly Color HostageColor = new(0.3f, 0.85f, 0.35f);
        static readonly Color BossColor = new(0.75f, 0.2f, 0.85f);

        [MenuItem("Aegis/Setup Stage1 Cuts In Active Scene")]
        public static void SetupActiveScene()
        {
            var stage1 = GameObject.Find(Stage1Path);
            if (stage1 == null)
            {
                Debug.LogError($"[Stage1SceneSetup] '{Stage1Path}' not found in active scene.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(stage1, "Setup Stage1 Cuts");

            RemoveLegacyPlaceholder(stage1.transform);
            var cutsRoot = GetOrCreate(cutsRoot: stage1.transform, CutsRootName);

            ClearChildren(cutsRoot.transform);

            BuildCut1_1(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_1_2_ShutterHalf", new Vector3(0f, 2f, -5f), "2:40-3:00 셔터 절반 개방");
            BuildCut1_2(cutsRoot.transform);
            BuildCut1_3(cutsRoot.transform);
            BuildCut1_4(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_2_3_ShutterFull", new Vector3(0f, 2f, 2f), "5:40-6:00 셔터 완전 개방");
            BuildCut1_5(cutsRoot.transform);
            BuildCut1_6(cutsRoot.transform);
            BuildTransition(cutsRoot.transform, "Transition_3_Boss_ApcEntrance", new Vector3(0f, 2f, 8f), "8:40-9:00 APC 진입");
            BuildCut1_7(cutsRoot.transform);

            SetupCameras(stage1.transform);
            EnsureStageRoot(stage1);
            WireMissionReferences();

            EditorUtility.SetDirty(stage1);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[Stage1SceneSetup] Stage1 cuts, spawns, and cameras configured.");
        }

        static void RemoveLegacyPlaceholder(Transform stage1)
        {
            var legacy = stage1.Find("enemy_Stage1_Lobby_01");
            if (legacy != null)
                Undo.DestroyObjectImmediate(legacy.gameObject);
        }

        static GameObject GetOrCreate(Transform cutsRoot, string name)
        {
            var existing = cutsRoot.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(cutsRoot, false);
            return go;
        }

        static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }

        static void BuildCut1_1(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_1_1_Entrance", new Vector3(0f, 0f, -12f), "1-1 로비 입구 | 그런트×4 L2 R2");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4.2f, 1f, -14.2f));
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-8.5f, 1f, -11.5f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(4.5f, 1f, -13.8f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(7.5f, 1f, -10.2f));

            CreateEnemy(cut.transform, "grunt_L_near_01", new Vector3(-4.2f, 1f, -14.2f), GruntColor);
            CreateEnemy(cut.transform, "grunt_L_far_02", new Vector3(-8.5f, 1.15f, -11.5f), GruntColor);
            CreateEnemy(cut.transform, "grunt_R_near_01", new Vector3(4.5f, 1f, -13.8f), GruntColor);
            CreateEnemy(cut.transform, "grunt_R_far_02", new Vector3(7.5f, 1.1f, -10.2f), GruntColor);
        }

        static void BuildCut1_2(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_1_2_Reception", new Vector3(0f, 0f, -6f), "1-2 안내 데스크 | 그런트×3 방패×1 인질×1");
            CreateSpawnSlot(cut.transform, "spawn_C_near", new Vector3(0f, 1f, -5.0f));
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4.2f, 1f, -7.8f));

            CreateEnemy(cut.transform, "grunt_desk_01", new Vector3(-4.2f, 1f, -7.8f), GruntColor);
            CreateEnemy(cut.transform, "grunt_desk_02", new Vector3(3.8f, 1f, -6.5f), GruntColor);
            CreateEnemy(cut.transform, "grunt_desk_03", new Vector3(-1.2f, 1f, -8.5f), GruntColor);
            CreateEnemy(cut.transform, "shield_C_near", new Vector3(0.3f, 1f, -4.8f), ShieldColor, new Vector3(0.8f, 1.2f, 0.4f));
            CreateEnemy(cut.transform, "hostage_desk_clerk", new Vector3(0.8f, 0.9f, -5.6f), HostageColor, new Vector3(0.5f, 1.6f, 0.4f));
        }

        static void BuildCut1_3(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_1_3_Balcony", new Vector3(0f, 0f, -2f), "1-3 발코니 앙각 | 저격×2 드론×2");
            CreateSpawnSlot(cut.transform, "spawn_L_far", new Vector3(-8f, Stage1LobbyDimensions.BalconyHeight + 0.2f, -2.5f));
            CreateSpawnSlot(cut.transform, "spawn_R_far", new Vector3(8f, Stage1LobbyDimensions.BalconyHeight + 0.2f, 2.0f));
            CreateSpawnSlot(cut.transform, "spawn_air_R", new Vector3(7f, Stage1LobbyDimensions.DroneAirHeight, -3f));
            CreateSpawnSlot(cut.transform, "spawn_air_L", new Vector3(-5f, Stage1LobbyDimensions.DroneAirHeight - 0.4f, 2.5f));

            CreateEnemy(cut.transform, "sniper_L_far", new Vector3(-8f, Stage1LobbyDimensions.BalconyHeight + 0.2f, -2.5f), SniperColor);
            CreateEnemy(cut.transform, "sniper_R_far", new Vector3(8f, Stage1LobbyDimensions.BalconyHeight + 0.2f, 2.0f), SniperColor);
            CreateEnemy(cut.transform, "drone_air_R", new Vector3(7f, Stage1LobbyDimensions.DroneAirHeight, -3f), DroneColor, new Vector3(0.5f, 0.3f, 0.5f));
            CreateEnemy(cut.transform, "drone_air_L", new Vector3(-5f, Stage1LobbyDimensions.DroneAirHeight - 0.4f, 2.5f), DroneColor, new Vector3(0.5f, 0.3f, 0.5f));
        }

        static void BuildCut1_4(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_1_4_Corridor", new Vector3(0f, 0f, -2f), "1-4 기둥 복도 | 그런트×5 인질×1");
            var columnSpawns = new[]
            {
                ("spawn_L_near", new Vector3(-10f, 1f, -6.5f)),
                ("spawn_C_far", new Vector3(0.5f, 1f, -2.5f)),
                ("spawn_R_near", new Vector3(10f, 1f, -4.5f)),
                ("spawn_L_far", new Vector3(-10f, 1f, 3.5f)),
            };
            foreach (var (name, pos) in columnSpawns)
                CreateSpawnSlot(cut.transform, name, pos);

            CreateEnemy(cut.transform, "grunt_col_L_01", new Vector3(-10f, 1f, -6.5f), GruntColor);
            CreateEnemy(cut.transform, "grunt_col_C_02", new Vector3(0.5f, 1f, -2.5f), GruntColor);
            CreateEnemy(cut.transform, "grunt_col_R_03", new Vector3(10f, 1f, -4.5f), GruntColor);
            CreateEnemy(cut.transform, "grunt_col_L_04", new Vector3(-10f, 1.1f, 3.5f), GruntColor);
            CreateEnemy(cut.transform, "grunt_col_R_05", new Vector3(10f, 1f, 5.5f), GruntColor);
            CreateEnemy(cut.transform, "hostage_col_R", new Vector3(8.5f, 0.9f, -3.8f), HostageColor, new Vector3(0.5f, 1.6f, 0.4f));
        }

        static void BuildCut1_5(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_1_5_ElevatorLobby", new Vector3(0f, 0f, 10f), "1-5 엘리베이터 로비 | 실드엘리트×2 그런트×2");
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4.5f, 1f, 12.2f));
            CreateSpawnSlot(cut.transform, "spawn_C_near", new Vector3(0f, 1f, 13.5f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(4.5f, 1f, 12.2f));

            CreateEnemy(cut.transform, "shield_elite_L", new Vector3(-4.5f, 1f, 12.2f), ShieldColor, new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "shield_elite_R", new Vector3(4.5f, 1f, 12.2f), ShieldColor, new Vector3(0.9f, 1.3f, 0.5f));
            CreateEnemy(cut.transform, "grunt_elev_01", new Vector3(-1.8f, 1f, 13.8f), GruntColor);
            CreateEnemy(cut.transform, "grunt_elev_02", new Vector3(1.8f, 1f, 13.8f), GruntColor);
        }

        static void BuildCut1_6(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_1_6_ParkingLot", new Vector3(0f, 0f, 16f), "1-6 주차장 | 그런트×4 드론×3");
            foreach (var (side, x, z) in new[] { ("L", -5.5f, 15.5f), ("C", 0f, 14.2f), ("R", 5.5f, 15.8f) })
                CreateSpawnSlot(cut.transform, $"spawn_{side}_near", new Vector3(x, 1f, z));

            CreateEnemy(cut.transform, "grunt_park_L", new Vector3(-5.5f, 1f, 15.5f), GruntColor);
            CreateEnemy(cut.transform, "grunt_park_C_01", new Vector3(-1.2f, 1f, 14.2f), GruntColor);
            CreateEnemy(cut.transform, "grunt_park_C_02", new Vector3(1.2f, 1f, 16.8f), GruntColor);
            CreateEnemy(cut.transform, "grunt_park_R", new Vector3(5.5f, 1f, 15.8f), GruntColor);
            CreateEnemy(cut.transform, "drone_park_L", new Vector3(-3.5f, Stage1LobbyDimensions.DroneAirHeight + 0.5f, 17.5f), DroneColor, new Vector3(0.5f, 0.3f, 0.5f));
            CreateEnemy(cut.transform, "drone_park_C", new Vector3(0f, Stage1LobbyDimensions.DroneAirHeight, 18.5f), DroneColor, new Vector3(0.5f, 0.3f, 0.5f));
            CreateEnemy(cut.transform, "drone_park_R", new Vector3(4f, Stage1LobbyDimensions.DroneAirHeight - 0.8f, 16f), DroneColor, new Vector3(0.5f, 0.3f, 0.5f));
        }

        static void BuildCut1_7(Transform parent)
        {
            var cut = CreateCut(parent, "Cut_1_7_Boss_APC", new Vector3(0f, 0f, 8f), "1-7 APC 보스 | P1탑 P2유탄 P3운전석");
            CreateSpawnSlot(cut.transform, "spawn_C_far", new Vector3(0f, 2.5f, 10f));
            CreateSpawnSlot(cut.transform, "spawn_L_near", new Vector3(-4f, 1.8f, 9f));
            CreateSpawnSlot(cut.transform, "spawn_R_near", new Vector3(4f, 1.8f, 9f));

            CreateEnemy(cut.transform, "boss_apc_turret_C", new Vector3(0f, 2.8f, 18.2f), BossColor, new Vector3(1.2f, 0.8f, 1.2f));
            CreateEnemy(cut.transform, "boss_apc_grenade_L", new Vector3(-1.0f, 1.8f, 19.5f), BossColor, new Vector3(0.8f, 0.8f, 0.8f));
            CreateEnemy(cut.transform, "boss_apc_cockpit_R", new Vector3(1.0f, 1.2f, 19.5f), BossColor, new Vector3(1f, 1f, 1.2f));

            GameObject apcBody = null;
            var apcPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ARX_APC/scene.gltf");
            if (apcPrefab != null)
            {
                apcBody = (GameObject)PrefabUtility.InstantiatePrefab(apcPrefab);
                Undo.RegisterCreatedObjectUndo(apcBody, "Create APC Body");
                apcBody.name = "APC_Body_Visual";
                apcBody.transform.SetParent(cut.transform, false);
                apcBody.transform.localPosition = new Vector3(0f, 0.0f, 10f);
                apcBody.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                apcBody.transform.localScale = Vector3.one * 0.55f;
            }
            else
            {
                apcBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(apcBody, "Create APC Body");
                apcBody.name = "APC_Body_Visual";
                apcBody.transform.SetParent(cut.transform, false);
                apcBody.transform.localPosition = new Vector3(0f, 1.2f, 10f);
                apcBody.transform.localScale = new Vector3(4f, 2f, 6f);
                apcBody.GetComponent<Collider>().enabled = false;
                var renderer = apcBody.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = CreateMat(new Color(0.15f, 0.15f, 0.18f));
            }
        }

        static void BuildTransition(Transform parent, string name, Vector3 pos, string note)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            var label = new GameObject("Note");
            label.transform.SetParent(go.transform, false);
            label.transform.localPosition = Vector3.up * 2f;
        }

        static GameObject CreateCut(Transform parent, string name, Vector3 anchor, string note)
        {
            var cut = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(cut, "Create " + name);
            cut.transform.SetParent(parent, false);
            cut.transform.position = anchor;

            var trigger = cut.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(24f, 6f, 18f);
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
            gizmo.transform.localPosition = Vector3.zero;
            gizmo.transform.localScale = Vector3.one * 0.25f;
            var col = gizmo.GetComponent<Collider>();
            if (col != null)
                Object.DestroyImmediate(col);
            var r = gizmo.GetComponent<Renderer>();
            if (r != null)
                r.sharedMaterial = CreateMat(new Color(1f, 1f, 1f, 0.35f));
        }

        static void CreateEnemy(Transform parent, string name, Vector3 worldPos, Color color, Vector3? scale = null)
        {
            GameObject enemy = null;
            string prefabPath = null;

            if (name.Contains("hostage"))
                prefabPath = "Assets/Prefabs/Hostage_Prisoner/scene.gltf";
            else if (name.Contains("cockpit"))
                prefabPath = "Assets/Prefabs/APC_Cockpit/APC_Cockpit/scene.gltf";
            else if (!name.Contains("drone") && !name.Contains("apc"))
                prefabPath = "Assets/Prefabs/Soldier_Grunt/scene.gltf";

            GameObject prefab = !string.IsNullOrEmpty(prefabPath) 
                ? UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) 
                : null;

            if (prefab != null)
            {
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(enemy, "Create " + name);
                enemy.name = name;
                enemy.transform.SetParent(parent, false);
                
                Quaternion spawnRotation = enemy.transform.localRotation;
                enemy.transform.position = worldPos;
                
                if (name.Contains("hostage"))
                {
                    // Hostage prefab has correct upright axes; just apply Y-rotation relative to spawn
                    enemy.transform.localRotation = spawnRotation * Quaternion.Euler(0f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one;
                }
                else if (name.Contains("cockpit"))
                {
                    // Spaceship cockpit visual alignment and scaling to fit boss APC scale
                    enemy.transform.localRotation = spawnRotation * Quaternion.Euler(0f, 180f, 0f);
                    enemy.transform.localScale = Vector3.one * 0.15f;
                }
                else
                {
                    // Soldier prefab has mismatched axes; rotate X by -90 to keep it upright
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
                if (renderer != null)
                    renderer.sharedMaterial = CreateMat(color);
            }
        }

        static Material CreateMat(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard");
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

        static void SetupCameras(Transform stage1)
        {
            var cameraPath = stage1.Find("CameraPath_Stage1");
            if (cameraPath == null)
            {
                var pathGo = new GameObject("CameraPath_Stage1");
                Undo.RegisterCreatedObjectUndo(pathGo, "Create CameraPath_Stage1");
                pathGo.transform.SetParent(stage1, false);
                cameraPath = pathGo.transform;
            }

            var presets = new (string name, Vector3 pos, Vector3 lookAt, float fov)[]
            {
                ("Vcam_1_1_Entrance", new Vector3(-2.5f, 3.2f, -22f), new Vector3(-0.5f, 2.2f, -14f), 48f),
                ("Vcam_1_2_Reception", new Vector3(4.5f, 1.6f, -9.5f), new Vector3(0f, 1.5f, -6.5f), 42f),
                ("Vcam_1_3_Balcony", new Vector3(-6f, 1.2f, -3f), new Vector3(0f, Stage1LobbyDimensions.BalconyHeight, 1f), 50f),
                ("Vcam_1_4_Corridor", new Vector3(-4f, 3.2f, -12f), new Vector3(0f, 2.5f, 4f), 38f),
                ("Vcam_1_5_ElevatorLobby", new Vector3(-8f, 3.8f, 8f), new Vector3(0f, 3f, 13f), 42f),
                ("Vcam_1_6_ParkingLot", new Vector3(3f, 6f, 12f), new Vector3(0f, 1.8f, 19f), 46f),
                ("Vcam_1_7_Boss", new Vector3(0f, 1.4f, 2f), new Vector3(0f, 2.8f, 11f), 40f),
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

            EnsureStage1CameraController(cameraPath);
        }

        static void EnsureStage1CameraController(Transform cameraPath)
        {
            var ctrl = cameraPath.GetComponent<Stage1CameraController>();
            if (ctrl == null)
                ctrl = Undo.AddComponent<Stage1CameraController>(cameraPath.gameObject);

            var names = new[]
            {
                "Vcam_1_1_Entrance",
                "Vcam_1_2_Reception",
                "Vcam_1_3_Balcony",
                "Vcam_1_4_Corridor",
                "Vcam_1_5_ElevatorLobby",
                "Vcam_1_6_ParkingLot",
                "Vcam_1_7_Boss",
            };

            var so = new SerializedObject(ctrl);
            var camerasProp = so.FindProperty("virtualCameras");
            camerasProp.arraySize = names.Length;

            for (var i = 0; i < names.Length; i++)
            {
                CinemachineCamera? vcam = null;
                var child = cameraPath.Find(names[i]);
                if (child != null)
                    vcam = child.GetComponent<CinemachineCamera>();

                camerasProp.GetArrayElementAtIndex(i).objectReferenceValue = vcam;
            }

            var mission = Object.FindAnyObjectByType<AegisMissionController>();
            if (mission != null)
                so.FindProperty("missionController").objectReferenceValue = mission;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureStageRoot(GameObject stage1)
        {
            if (stage1.GetComponent<StageRoot>() == null)
                Undo.AddComponent<StageRoot>(stage1);

            var stageManager = Object.FindAnyObjectByType<StageManager>();
            if (stageManager != null)
            {
                var root = stage1.GetComponent<StageRoot>();
                var so = new SerializedObject(root);
                so.FindProperty("stageManager").objectReferenceValue = stageManager;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void WireMissionReferences()
        {
            var mission = Object.FindAnyObjectByType<AegisMissionController>();
            var cameraCtrl = Object.FindAnyObjectByType<Stage1CameraController>();

            if (mission != null && cameraCtrl != null)
            {
                var so = new SerializedObject(cameraCtrl);
                so.FindProperty("missionController").objectReferenceValue = mission;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var bootstrap = Object.FindAnyObjectByType<AegisMissionBootstrap>();
            if (bootstrap != null && mission != null)
            {
                var so = new SerializedObject(bootstrap);
                so.FindProperty("missionController").objectReferenceValue = mission;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
