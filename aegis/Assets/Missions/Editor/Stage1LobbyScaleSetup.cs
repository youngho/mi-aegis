#if UNITY_EDITOR
#nullable enable
using System.IO;
using PinkSoft.Aegis.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>Stage1 로비 층고·프리팹·카메라·외관 스케일을 일괄 적용합니다.</summary>
    public static class Stage1LobbyScaleSetup
    {
        const string KitFolder = "Assets/Prefabs/BuildingKit/Stage1Lobby";
        const string Stage1ScenePath = "Assets/Scenes/Stages/Stage1_Lobby.unity";

        [MenuItem("Aegis/Apply Stage1 Lobby Scale (Grand Atrium)")]
        public static void ApplyFromMenu()
        {
            ApplyBuildingKitPrefabs();
            ApplyEntrancePrefabHeights();

            if (File.Exists(Stage1ScenePath))
            {
                EditorSceneManager.OpenScene(Stage1ScenePath, OpenSceneMode.Single);
                Stage1SceneSetup.SetupActiveScene();
                Stage1LobbyArchitectureSetup.BuildArchitecture(GameObject.Find("Stage1_Lobby")!.transform);
                Stage1LobbyVisualSetup.ApplyToActiveScene();
                Stage1LobbyExteriorSetup.BuildExteriorAndPostProcess();
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Stage1LobbyScaleSetup] Lobby scaled to {Stage1LobbyDimensions.CeilingHeight}m ceiling.");
        }

        public static void ApplyBuildingKitPrefabs()
        {
            SetWallPrefab($"{KitFolder}/PF_Lobby_Wall_Front.prefab", new Vector3(0f, Stage1LobbyDimensions.WallCenterY, -15.2f));
            SetWallPrefab($"{KitFolder}/PF_Lobby_Wall_Back.prefab", new Vector3(0f, Stage1LobbyDimensions.WallCenterY, 15f));
            SetWallPrefab($"{KitFolder}/PF_Lobby_Wall_Left.prefab", new Vector3(-15f, Stage1LobbyDimensions.WallCenterY, 0f));
            SetWallPrefab($"{KitFolder}/PF_Lobby_Wall_Right.prefab", new Vector3(15f, Stage1LobbyDimensions.WallCenterY, 0f));

            SetSimplePrefab($"{KitFolder}/PF_Lobby_Ceiling.prefab",
                new Vector3(0f, Stage1LobbyDimensions.CeilingHeight, 0f), new Vector3(30f, 0.2f, 30f));

            SetSimplePrefab($"{KitFolder}/PF_Lobby_Column.prefab",
                new Vector3(10f, Stage1LobbyDimensions.ColumnCenterY, 10f),
                new Vector3(1f, Stage1LobbyDimensions.ColumnScaleY, 1f));

            SetSimplePrefab($"{KitFolder}/PF_Lobby_Shutter.prefab",
                new Vector3(-13f, Stage1LobbyDimensions.WallHeight * 0.25f, 0f),
                new Vector3(3f, Stage1LobbyDimensions.WallHeight * 0.5f, 0.2f));

            ApplySignPrefab();
        }

        static void SetWallPrefab(string path, Vector3 position)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            root.transform.localPosition = position;
            root.transform.localScale = new Vector3(
                root.transform.localScale.x,
                Stage1LobbyDimensions.WallHeight,
                root.transform.localScale.z);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void SetSimplePrefab(string path, Vector3 position, Vector3 scale)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            root.transform.localPosition = position;
            root.transform.localScale = scale;
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void ApplySignPrefab()
        {
            const string path = KitFolder + "/PF_Lobby_Sign.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name is "Sign_Panel" or "Sign_Glow")
                {
                    var p = t.localPosition;
                    p.y = Stage1LobbyDimensions.BackSignCenterY;
                    t.localPosition = p;
                    t.localScale = new Vector3(t.localScale.x, 2.4f, t.localScale.z);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void ApplyEntrancePrefabHeights()
        {
            const string path = KitFolder + "/PF_Lobby_Entrance.prefab";
            var factor = Stage1LobbyDimensions.ScaleFromLegacy;

            var root = PrefabUtility.LoadPrefabContents(path);
            var topFrame = root.transform.Find("DoorFrame_Top");
            if (topFrame != null && topFrame.localPosition.y > Stage1LobbyDimensions.LegacyCeilingHeight * 1.2f)
            {
                PrefabUtility.UnloadPrefabContents(root);
                Debug.Log("[Stage1LobbyScaleSetup] Entrance prefab already scaled; skipped.");
                return;
            }

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t == root.transform)
                    continue;

                var p = t.localPosition;
                p.y *= factor;
                t.localPosition = p;

                var s = t.localScale;
                s.y *= factor;
                t.localScale = s;
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
