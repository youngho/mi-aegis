#if UNITY_EDITOR
using System.IO;
using System.Linq;
using PinkSoft.Aegis.Missions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PinkSoft.Aegis.Editor
{
    public static class AegisAddressablesSetup
    {
        const string PrefabPath = "Assets/Missions/Prefabs/AegisMission.prefab";
        const string AddressableAddress = "aegis_rail_shooter";
        const string MissionGroupName = "Aegis_Mission";
        const string StagesGroupName = "Aegis_Stages";

        [MenuItem("PinkSoft/AEGIS/Setup Mission (Prefab + Addressables)")]
        public static void SetupMission()
        {
            EnsureFolder("Assets/Missions/Prefabs");
            EnsureFolder("Assets/Stages");

            var prefab = CreateOrUpdateMissionPrefab();
            RegisterMissionAddressable(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AEGIS] Mission prefab ready at {PrefabPath}, addressable address='{AddressableAddress}'.");
        }

        [MenuItem("PinkSoft/AEGIS/Create Stage Placeholder Scenes")]
        public static void CreateStageScenes()
        {
            EnsureFolder("Assets/Stages");

            var stageNames = new[]
            {
                "Stage1_Lobby",
                "Stage2_Lab",
                "Stage3_Datacenter",
                "Stage4_Core"
            };

            foreach (var stageName in stageNames)
            {
                var scenePath = $"Assets/Stages/{stageName}.unity";
                if (File.Exists(scenePath))
                    continue;

                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

                CreatePlaceholderTarget(stageName, new Vector3(0f, 1f, 5f));
                CreatePlaceholderTarget($"{stageName}_boss", new Vector3(2f, 1.5f, 7f));

                EditorSceneManager.SaveScene(scene, scenePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AEGIS] Stage placeholder scenes created under Assets/Stages/.");
        }

        [MenuItem("PinkSoft/AEGIS/Create Dev Test Scene")]
        public static void CreateDevTestScene()
        {
            EnsureFolder("Assets/Scenes");

            var scenePath = "Assets/Scenes/AegisMissionDev.unity";
            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"[AEGIS] Dev scene already exists: {scenePath}");
                return;
            }

            SetupMission();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var mission = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            mission.name = "AegisMission";

            var bootstrapGo = new GameObject("AegisMissionBootstrap");
            bootstrapGo.AddComponent<AegisMissionBootstrap>();
            bootstrapGo.AddComponent<DebugMissionInput>();

            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AEGIS] Dev test scene created: {scenePath}");
        }

        [MenuItem("PinkSoft/AEGIS/Build Addressables")]
        public static void BuildAddressablesMenu()
        {
            EditorApplication.delayCall += BuildAddressables;
            Debug.Log("[AEGIS] Addressables build scheduled.");
        }

        public static void BuildAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (settings == null)
            {
                Debug.LogError("[AEGIS] AddressableAssetSettings not found.");
                return;
            }

            AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent(out var result);
            if (!string.IsNullOrEmpty(result.Error))
                Debug.LogError($"[AEGIS] Addressables build failed: {result.Error}");
            else
                Debug.Log($"[AEGIS] Addressables build succeeded. Output: {settings.RemoteCatalogBuildPath}");
        }

        [MenuItem("PinkSoft/AEGIS/Copy Bundle To mi StreamingAssets")]
        public static void CopyBundleToMi()
        {
            const string version = "1.0.0";
            var bundleName = $"{AddressableAddress}_{version}.bundle";
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
                return;

            var searchRoots = new[]
            {
                Path.Combine(projectRoot, "ServerData"),
                Path.Combine(projectRoot, "Library", "com.unity.addressables", "aa"),
                Path.Combine(projectRoot, "Library", "com.unity.addressables", "aa", "OSX"),
            };

            string source = null;
            foreach (var root in searchRoots)
            {
                if (!Directory.Exists(root))
                    continue;
                source = Directory.GetFiles(root, "*.bundle", SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .FirstOrDefault();
                if (source != null)
                    break;
            }

            if (source == null)
            {
                Debug.LogError("[AEGIS] No .bundle found. Run Build Addressables first.");
                return;
            }

            var destDir = Path.GetFullPath(Path.Combine(projectRoot, "..", "mi", "Assets", "StreamingAssets", "Missions"));
            Directory.CreateDirectory(destDir);
            var dest = Path.Combine(destDir, bundleName);
            File.Copy(source, dest, true);
            Debug.Log($"[AEGIS] Copied bundle to mi: {dest}");
        }

        static GameObject CreateOrUpdateMissionPrefab()
        {
            var root = new GameObject("AegisMission");
            root.AddComponent<AegisMissionController>();
            var stageManager = root.AddComponent<StageManager>();

            var stagesParent = new GameObject("Stages");
            stagesParent.transform.SetParent(root.transform, false);

            var stageObjects = new GameObject[4];
            var stageNames = new[] { "Stage1_Lobby", "Stage2_Lab", "Stage3_Datacenter", "Stage4_Core" };
            for (var i = 0; i < stageNames.Length; i++)
            {
                var stage = new GameObject(stageNames[i]);
                stage.transform.SetParent(stagesParent.transform, false);
                stage.AddComponent<StageRoot>();
                CreatePlaceholderTarget($"enemy_{stageNames[i]}_01", new Vector3(0f, 1f, 5f), stage.transform);
                stage.SetActive(false);
                stageObjects[i] = stage;
            }

            var serializedStageManager = new SerializedObject(stageManager);
            serializedStageManager.FindProperty("stages").arraySize = stageObjects.Length;
            for (var i = 0; i < stageObjects.Length; i++)
                serializedStageManager.FindProperty("stages").GetArrayElementAtIndex(i).objectReferenceValue = stageObjects[i];
            serializedStageManager.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void RegisterMissionAddressable(GameObject prefab)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[AEGIS] AddressableAssetSettings not found. Install com.unity.addressables first.");
                return;
            }

            var missionGroup = GetOrCreateGroup(settings, MissionGroupName, true);
            GetOrCreateGroup(settings, StagesGroupName, true);

            var assetPath = AssetDatabase.GetAssetPath(prefab);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, missionGroup);
            entry.address = AddressableAddress;
            entry.SetLabel("mission_aegis", true, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }

        static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName, bool remote)
        {
            var group = settings.FindGroup(groupName);
            if (group != null)
                return group;

            var schemas = settings.DefaultGroup.Schemas;
            group = settings.CreateGroup(groupName, false, false, true, schemas);
            if (remote)
            {
                var bundleSchema = group.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
                if (bundleSchema != null)
                    bundleSchema.BuildPath.SetVariableByName(settings, "RemoteBuildPath");
            }

            return group;
        }

        static void CreatePlaceholderTarget(string name, Vector3 localPosition, Transform parent = null)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = name;
            target.transform.SetParent(parent, false);
            target.transform.localPosition = localPosition;
            target.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            target.layer = LayerMask.NameToLayer("Default");
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
    }
}
#endif
