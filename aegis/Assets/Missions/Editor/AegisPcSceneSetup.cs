#if UNITY_EDITOR
using System.IO;
using PinkSoft.Aegis.Missions;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PinkSoft.Aegis.Missions.Editor
{
    /// <summary>모든 플레이 씬에 PC 조작·미션 셸·StageRoot를 일괄 구성합니다.</summary>
    public static class AegisPcSceneSetup
    {
        const string MissionPrefabPath = "Assets/Missions/Prefabs/AegisMission.prefab";

        static readonly (string path, string rootName)[] StageScenes =
        {
            ("Assets/Scenes/Stages/Stage1_Lobby.unity", "Stage1_Lobby"),
            ("Assets/Scenes/Stages/Stage2_Lab.unity", "Stage2_Lab"),
            ("Assets/Scenes/Stages/Stage3_Datacenter.unity", "Stage3_Datacenter"),
            ("Assets/Scenes/Stages/Stage4_Core.unity", "Stage4_Core"),
        };

        [MenuItem("Aegis/Ensure PC Play Setup In All Scenes")]
        public static void EnsureAllScenes()
        {
            StageArchitectureMigration.SetupStage1LobbyPlayScene();

            foreach (var (path, rootName) in StageScenes)
            {
                if (path.Contains("Stage1_Lobby"))
                    continue;

                if (!File.Exists(path))
                    continue;

                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                EnsureStageSceneBasics(rootName);
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[AegisPcSceneSetup] PC play setup applied to all stage scenes. Briefing & AegisMissionFull use runtime AegisPlayModeServices.");
        }

        static void EnsureStageSceneBasics(string stageRootName)
        {
            EnsureMainCameraWithBrain();
            EnsureStageRoot(stageRootName);
        }

        static void EnsureMainCameraWithBrain()
        {
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            if (camGo.GetComponent<CinemachineBrain>() == null)
                camGo.AddComponent<CinemachineBrain>();
        }

        static void EnsureStageRoot(string stageRootName)
        {
            var root = GameObject.Find(stageRootName);
            if (root == null)
            {
                Debug.LogWarning($"[AegisPcSceneSetup] Root '{stageRootName}' not found; skipped StageRoot.");
                return;
            }

            if (root.GetComponent<StageRoot>() == null)
                root.AddComponent<StageRoot>();
        }
    }
}
#endif
