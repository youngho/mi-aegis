#if UNITY_EDITOR
#nullable enable
using System.IO;
using System.Linq;
using PinkSoft.Aegis.Missions;
using PinkSoft.Aegis.Missions.Editor;
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
            StageArchitectureMigration.RebuildSlimMissionPrefab();
            StageArchitectureMigration.PrepareStagePlaceholderScenes();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            RegisterMissionAddressable(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AEGIS] Slim mission prefab at {PrefabPath}, addressable address='{AddressableAddress}'.");
        }

        [MenuItem("PinkSoft/AEGIS/Create Stage Placeholder Scenes")]
        public static void CreateStageScenes()
        {
            StageArchitectureMigration.PrepareStagePlaceholderScenes();
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

            string? source = null;
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

            var destDir = Path.GetFullPath(Path.Combine(projectRoot, "..", "..", "mi", "Assets", "StreamingAssets", "Missions"));
            Directory.CreateDirectory(destDir);
            var dest = Path.Combine(destDir, bundleName);
            File.Copy(source, dest, true);
            Debug.Log($"[AEGIS] Copied bundle to mi: {dest}");
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
