#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>BuildingKit 프리팹으로 Stage1 로비 환경을 구성합니다.</summary>
    public static class Stage1EnvironmentSetup
    {
        const string KitFolder = "Assets/Prefabs/BuildingKit/Stage1Lobby";
        const string EnvironmentName = "Environment_Stage1_Lobby";
        const string CameraPathName = "CameraPath_Stage1";

        static readonly string[] KitPrefabs =
        {
            "PF_Lobby_Floor",
            "PF_Lobby_Ceiling",
            "PF_Lobby_Wall_Front",
            "PF_Lobby_Wall_Back",
            "PF_Lobby_Wall_Left",
            "PF_Lobby_Wall_Right",
            "PF_Lobby_Entrance",
            "PF_Lobby_ReceptionDesk",
            "PF_Lobby_Shutter",
            "PF_Lobby_Sign",
        };

        static readonly Vector3[] ExtraColumnPositions =
        {
            new(-10f, 2.5f, -5f),
            new(10f, 2.5f, -5f),
            new(-10f, 2.5f, 2f),
            new(10f, 2.5f, 2f),
            new(-10f, 2.5f, 10f),
            new(10f, 2.5f, 10f),
        };

        public static void RepairStage1Lobby(Transform stage1)
        {
            RemoveBrokenChildren(stage1);
            EnsureEnvironment(stage1);
            EnsureCameraPath(stage1);
        }

        static void RemoveBrokenChildren(Transform stage1)
        {
            for (var i = stage1.childCount - 1; i >= 0; i--)
            {
                var child = stage1.GetChild(i);
                if (child.name.Contains("Missing Prefab"))
                    Undo.DestroyObjectImmediate(child.gameObject);
            }

            var env = stage1.Find(EnvironmentName);
            if (env != null && env.childCount == 0 && PrefabUtility.IsPartOfPrefabInstance(env.gameObject))
                Undo.DestroyObjectImmediate(env.gameObject);

            var cuts = stage1.Find("Stage1_Cuts");
            if (cuts != null && cuts.childCount == 0)
                Undo.DestroyObjectImmediate(cuts.gameObject);
        }

        static void EnsureEnvironment(Transform stage1)
        {
            var env = stage1.Find(EnvironmentName);
            if (env != null && env.childCount > 0)
                return;

            if (env != null)
                Undo.DestroyObjectImmediate(env.gameObject);

            var envGo = new GameObject(EnvironmentName);
            Undo.RegisterCreatedObjectUndo(envGo, "Create " + EnvironmentName);
            envGo.transform.SetParent(stage1, false);
            envGo.transform.localPosition = Vector3.zero;
            envGo.transform.localRotation = Quaternion.identity;
            envGo.transform.localScale = Vector3.one;

            foreach (var prefabName in KitPrefabs)
                InstantiateKitPrefab(prefabName, envGo.transform);

            var columnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{KitFolder}/PF_Lobby_Column.prefab");
            if (columnPrefab != null)
            {
                InstantiateKitPrefab("PF_Lobby_Column", envGo.transform);
                foreach (var pos in ExtraColumnPositions)
                {
                    var col = (GameObject)PrefabUtility.InstantiatePrefab(columnPrefab, envGo.transform);
                    col.name = "PF_Lobby_Column";
                    col.transform.localPosition = pos;
                }
            }
        }

        static void EnsureCameraPath(Transform stage1)
        {
            var path = stage1.Find(CameraPathName);
            if (path != null && path.childCount > 0)
                return;

            if (path != null)
                Undo.DestroyObjectImmediate(path.gameObject);

            var pathGo = new GameObject(CameraPathName);
            Undo.RegisterCreatedObjectUndo(pathGo, "Create " + CameraPathName);
            pathGo.transform.SetParent(stage1, false);
            pathGo.transform.localPosition = Vector3.zero;
            pathGo.transform.localRotation = Quaternion.identity;
            pathGo.transform.localScale = Vector3.one;
        }

        static void InstantiateKitPrefab(string prefabName, Transform parent)
        {
            var path = $"{KitFolder}/{prefabName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[Stage1EnvironmentSetup] Missing prefab: {path}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = prefabName;
        }
    }
}
#endif
