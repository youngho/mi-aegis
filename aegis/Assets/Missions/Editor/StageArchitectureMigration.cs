#if UNITY_EDITOR
#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PinkSoft.Aegis.Missions;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>스테이지별 씬 분리 아키텍처로 마이그레이션.</summary>
    public static class StageArchitectureMigration
    {
        const string MissionPrefabPath = "Assets/Missions/Prefabs/AegisMission.prefab";
        const string Mission1ScenePath = "Assets/Scenes/AegisMission1.unity";
        const string MissionFullScenePath = "Assets/Scenes/AegisMissionFull.unity";
        const string Stage1ScenePath = "Assets/Scenes/Stages/Stage1_Lobby.unity";
        const string Stage1SourcePath = "AegisMission/Stages/Stage1_Lobby";

        static readonly (string scenePath, string address, string rootName)[] StageDefs =
        {
            ("Assets/Scenes/Stages/Stage1_Lobby.unity", "stage/1_lobby", "Stage1_Lobby"),
            ("Assets/Scenes/Stages/Stage2_Lab.unity", "stage/2_lab", "Stage2_Lab"),
            ("Assets/Scenes/Stages/Stage3_Datacenter.unity", "stage/3_datacenter", "Stage3_Datacenter"),
            ("Assets/Scenes/Stages/Stage4_Core.unity", "stage/4_core", "Stage4_Core"),
        };

        [MenuItem("Aegis/Migrate Stage Architecture")]
        public static void MigrateAll()
        {
            RebuildSlimMissionPrefab();
            RegisterStageScenesInAddressables();
            SetupStage1LobbyPlayScene();
            RebuildMissionFullScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[StageArchitectureMigration] Complete. Stage1_Lobby = play scene, AegisMissionFull = all stages.");
        }

        /// <summary>Stage2_Lab 환경·컷·카메라 재구성.</summary>
        [MenuItem("Aegis/Repair Stage2 Lab Scene")]
        public static void RepairStage2LabScene()
        {
            const string stage2Path = "Assets/Scenes/Stages/Stage2_Lab.unity";
            if (!File.Exists(stage2Path))
            {
                Debug.LogError($"[StageArchitectureMigration] Missing {stage2Path}");
                return;
            }

            EditorSceneManager.OpenScene(stage2Path, OpenSceneMode.Single);
            Stage2SceneSetup.SetupActiveScene();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[StageArchitectureMigration] Stage2_Lab repaired.");
        }

        /// <summary>깨진 프리팹 참조 제거 + BuildingKit 환경·컷·카메라 재구성.</summary>
        [MenuItem("Aegis/Repair Stage1 Lobby Scene")]
        public static void RepairStage1LobbyScene()
        {
            if (!File.Exists(Stage1ScenePath))
            {
                Debug.LogError($"[StageArchitectureMigration] Missing {Stage1ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
            var stage1Root = GameObject.Find("Stage1_Lobby");
            if (stage1Root == null)
            {
                Debug.LogError("[StageArchitectureMigration] Stage1_Lobby root not found.");
                return;
            }

            Stage1EnvironmentSetup.RepairStage1Lobby(stage1Root.transform);
            Stage1SceneSetup.SetupActiveScene();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[StageArchitectureMigration] Stage1_Lobby repaired.");
        }

        /// <summary>Stage1_Lobby에 미션 셸·Bootstrap을 넣고 AegisMission1.unity를 제거합니다.</summary>
        [MenuItem("Aegis/Setup Stage1 Lobby Play Scene")]
        public static void SetupStage1LobbyPlayScene()
        {
            if (!File.Exists(Stage1ScenePath))
            {
                Debug.LogError($"[StageArchitectureMigration] Missing {Stage1ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
            var scene = SceneManager.GetActiveScene();

            var stage1Root = GameObject.Find("Stage1_Lobby");
            if (stage1Root != null)
            {
                Stage1EnvironmentSetup.RepairStage1Lobby(stage1Root.transform);
                Stage1SceneSetup.SetupActiveScene();
            }

            var boss = GameObject.Find("Stage1_Lobby_boss");
            if (boss != null)
                Object.DestroyImmediate(boss);

            EnsureMainCameraForMission();

            AegisMissionController? missionController = null;
            var missionGo = GameObject.Find("AegisMission");
            if (missionGo == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MissionPrefabPath);
                missionGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                missionGo.name = "AegisMission";
            }

            var stageManager = missionGo.GetComponent<StageManager>();
            if (stageManager != null)
                ConfigureStageManager(stageManager, StageSceneAddresses.Stage1Only, StageSceneAddresses.Stage1OnlyEditorPaths);

            missionController = missionGo.GetComponent<AegisMissionController>();

            if (Object.FindAnyObjectByType<AegisMissionBootstrap>() == null)
            {
                var bootstrapGo = new GameObject("AegisMissionBootstrap");
                SceneManager.MoveGameObjectToScene(bootstrapGo, scene);
                var debugInput = bootstrapGo.AddComponent<DebugMissionInput>();
                var bootstrap = bootstrapGo.AddComponent<AegisMissionBootstrap>();
                var bootstrapSo = new SerializedObject(bootstrap);
                bootstrapSo.FindProperty("missionController").objectReferenceValue = missionController;
                bootstrapSo.FindProperty("debugInput").objectReferenceValue = debugInput;
                bootstrapSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var cameraCtrl = Object.FindAnyObjectByType<Stage1CameraController>();
            if (cameraCtrl != null && missionController != null)
            {
                var cameraSo = new SerializedObject(cameraCtrl);
                cameraSo.FindProperty("missionController").objectReferenceValue = missionController;
                cameraSo.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            if (File.Exists(Mission1ScenePath))
                AssetDatabase.DeleteAsset(Mission1ScenePath);

            Debug.Log("[StageArchitectureMigration] Stage1_Lobby play scene ready. AegisMission1.unity removed if present.");
        }

        static void MigrateStage1ContentFromMission1()
        {
            if (!File.Exists(Mission1ScenePath))
            {
                Debug.LogWarning("[StageArchitectureMigration] AegisMission1.unity not found; skip Stage1 migration.");
                return;
            }

            EnsureFolder("Assets/Missions/Editor/Temp");
            var tempPaths = new List<string>();

            try
            {
                EditorSceneManager.OpenScene(Mission1ScenePath, OpenSceneMode.Single);
                var stage1Source = GameObject.Find(Stage1SourcePath);
                if (stage1Source == null)
                {
                    Debug.LogWarning("[StageArchitectureMigration] Stage1 source not found in AegisMission1.");
                    return;
                }

                var childNames = new[] { "Environment_Stage1_Lobby", "CameraPath_Stage1", "Stage1_Cuts" };
                foreach (var childName in childNames)
                {
                    var child = stage1Source.transform.Find(childName);
                    if (child == null)
                        continue;

                    var tempPath = $"Assets/Missions/Editor/Temp/{childName}.prefab";
                    PrefabUtility.SaveAsPrefabAsset(child.gameObject, tempPath);
                    tempPaths.Add(tempPath);
                }

                if (tempPaths.Count == 0)
                {
                    Debug.LogWarning("[StageArchitectureMigration] No Stage1 children to migrate.");
                    return;
                }

                EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
                var activeScene = SceneManager.GetActiveScene();
                var root = PrepareStageRoot("Stage1_Lobby", activeScene);

                foreach (var tempPath in tempPaths)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(tempPath);
                    if (asset == null)
                        continue;

                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
                    instance.name = asset.name;
                }

                EnsureMainCameraForMission();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log($"[StageArchitectureMigration] Migrated {tempPaths.Count} Stage1 roots to {Stage1ScenePath}.");
            }
            finally
            {
                foreach (var tempPath in tempPaths)
                {
                    if (AssetDatabase.LoadAssetAtPath<Object>(tempPath) != null)
                        AssetDatabase.DeleteAsset(tempPath);
                }
            }
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static GameObject PrepareStageRoot(string rootName, Scene scene)
        {
            GameObject? root = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == rootName)
                {
                    root = go;
                    break;
                }
            }

            if (root == null)
            {
                root = new GameObject(rootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            foreach (var renderer in root.GetComponents<MeshRenderer>())
                Object.DestroyImmediate(renderer);
            foreach (var filter in root.GetComponents<MeshFilter>())
                Object.DestroyImmediate(filter);
            foreach (var collider in root.GetComponents<Collider>())
                Object.DestroyImmediate(collider);

            if (root.GetComponent<StageRoot>() == null)
                root.AddComponent<StageRoot>();

            // Remove placeholder children (boss cube etc.)
            for (var i = root.transform.childCount - 1; i >= 0; i--)
            {
                var child = root.transform.GetChild(i);
                if (child.name.EndsWith("_boss") || child.name.StartsWith("enemy_"))
                    Object.DestroyImmediate(child.gameObject);
            }

            return root;
        }

        static void EnsureMainCameraForMission()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            cam.transform.position = new Vector3(0f, 2f, -10f);
            cam.transform.rotation = Quaternion.Euler(6f, 0f, 0f);
            if (cam.GetComponent<CinemachineBrain>() == null)
                cam.gameObject.AddComponent<CinemachineBrain>();
        }

        static void ConfigureStageManager(StageManager stageManager, string[] addresses, string[] editorPaths)
        {
            var so = new SerializedObject(stageManager);
            so.FindProperty("stageSceneAddresses").arraySize = addresses.Length;
            for (var i = 0; i < addresses.Length; i++)
                so.FindProperty("stageSceneAddresses").GetArrayElementAtIndex(i).stringValue = addresses[i];

            so.FindProperty("editorScenePaths").arraySize = editorPaths.Length;
            for (var i = 0; i < editorPaths.Length; i++)
                so.FindProperty("editorScenePaths").GetArrayElementAtIndex(i).stringValue = editorPaths[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void RebuildSlimMissionPrefab()
        {
            var root = new GameObject("AegisMission");
            var controller = root.AddComponent<AegisMissionController>();
            var stageManager = root.AddComponent<StageManager>();

            var so = new SerializedObject(stageManager);
            so.FindProperty("stageSceneAddresses").arraySize = StageSceneAddresses.All.Length;
            for (var i = 0; i < StageSceneAddresses.All.Length; i++)
                so.FindProperty("stageSceneAddresses").GetArrayElementAtIndex(i).stringValue = StageSceneAddresses.All[i];

            so.FindProperty("editorScenePaths").arraySize = StageSceneAddresses.EditorPaths.Length;
            for (var i = 0; i < StageSceneAddresses.EditorPaths.Length; i++)
                so.FindProperty("editorScenePaths").GetArrayElementAtIndex(i).stringValue = StageSceneAddresses.EditorPaths[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("stageManager").objectReferenceValue = stageManager;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, MissionPrefabPath);
            Object.DestroyImmediate(root);
        }

        static void RegisterStageScenesInAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[StageArchitectureMigration] AddressableAssetSettings not found.");
                return;
            }

            var group = settings.FindGroup("Aegis_Stages")
                        ?? settings.CreateGroup("Aegis_Stages", false, false, true, settings.DefaultGroup.Schemas);

            foreach (var (scenePath, address, _) in StageDefs)
            {
                if (!File.Exists(scenePath))
                    continue;

                var guid = AssetDatabase.AssetPathToGUID(scenePath);
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = address;
                entry.SetLabel("stage", true, true);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        }

        static void RebuildMissionFullScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            SetupMissionShell(scene, StageSceneAddresses.All, StageSceneAddresses.EditorPaths);
            EditorSceneManager.SaveScene(scene, MissionFullScenePath);
        }

        static void SetupMissionShell(Scene scene, string[] addresses, string[] editorPaths)
        {
            EnsureMainCameraForMission();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MissionPrefabPath);
            var mission = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            mission.name = "AegisMission";
            SceneManager.MoveGameObjectToScene(mission, scene);

            var stageManager = mission.GetComponent<StageManager>();
            if (stageManager != null)
                ConfigureStageManager(stageManager, addresses, editorPaths);

            var bootstrapGo = new GameObject("AegisMissionBootstrap");
            SceneManager.MoveGameObjectToScene(bootstrapGo, scene);
            bootstrapGo.AddComponent<AegisMissionBootstrap>();
            bootstrapGo.AddComponent<DebugMissionInput>();

            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Aegis/Strip Stage Scene Shell Objects")]
        public static void StripStageSceneShellObjects()
        {
            foreach (var (scenePath, _, _) in StageDefs)
            {
                if (!File.Exists(scenePath))
                    continue;

                // Stage1_Lobby keeps Main Camera for standalone play-test.
                var stripCamera = scenePath != Stage1ScenePath;

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var scene = SceneManager.GetActiveScene();

                foreach (var root in scene.GetRootGameObjects())
                {
                    if (!stripCamera && root.name is "Main Camera")
                        continue;

                    if (root.name is "Main Camera" or "Directional Light" or "AegisMission" or "AegisMissionBootstrap")
                        Object.DestroyImmediate(root);
                }

                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("[StageArchitectureMigration] Stripped shell objects from stage scenes (Stage1 keeps Main Camera for standalone play).");
        }

        [MenuItem("Aegis/Prepare Stage Placeholder Scenes")]
        public static void PrepareStagePlaceholderScenes()
        {
            foreach (var (scenePath, _, rootName) in StageDefs)
            {
                if (scenePath == Stage1ScenePath && File.Exists(scenePath))
                {
                    // Stage1 may already have real content
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    var root = PrepareStageRoot(rootName, scene);
                    if (root.GetComponent<StageRoot>() == null)
                        root.AddComponent<StageRoot>();
                    EditorSceneManager.SaveScene(scene);
                    continue;
                }

                if (!File.Exists(scenePath))
                {
                    var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    var root = new GameObject(rootName);
                    root.AddComponent<StageRoot>();
                    CreatePlaceholderTarget($"enemy_{rootName}_01", new Vector3(0f, 1f, 5f), root.transform);
                    EditorSceneManager.SaveScene(newScene, scenePath);
                }
                else
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    var root = PrepareStageRoot(rootName, scene);
                    if (root.transform.childCount == 0)
                        CreatePlaceholderTarget($"enemy_{rootName}_01", new Vector3(0f, 1f, 5f), root.transform);
                    if (root.GetComponent<StageRoot>() == null)
                        root.AddComponent<StageRoot>();
                    EditorSceneManager.SaveScene(scene);
                }
            }

            RegisterStageScenesInAddressables();
            AssetDatabase.SaveAssets();
            Debug.Log("[StageArchitectureMigration] Stage placeholder scenes prepared.");
        }

        static void CreatePlaceholderTarget(string name, Vector3 localPosition, Transform parent)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = name;
            target.transform.SetParent(parent, false);
            target.transform.localPosition = localPosition;
            target.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
    }
}
#endif
