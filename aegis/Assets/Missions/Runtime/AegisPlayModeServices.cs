#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PinkSoft.Aegis.Missions
{
    /// <summary>모든 씬 Play 모드에서 PC 단축키·도움말·(에디터) 스테이지 자동 부트스트랩을 제공합니다.</summary>
    public static class AegisPlayModeServices
    {
        const string HostName = "AegisPlayModeServices";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying || Object.FindAnyObjectByType<AegisPlayModeServicesHost>() != null)
                return;

            var hostGo = new GameObject(HostName);
            hostGo.AddComponent<AegisPlayModeServicesHost>();
            Object.DontDestroyOnLoad(hostGo);
        }
    }

    public sealed class AegisPlayModeServicesHost : MonoBehaviour
    {
        bool _showHelp = true;
        float _helpHideTimer;
        bool _reloading;

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            StartCoroutine(BootstrapAfterSceneLoad());
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _showHelp = true;
            _helpHideTimer = 8f;
            StartCoroutine(BootstrapAfterSceneLoad());
        }

        IEnumerator BootstrapAfterSceneLoad()
        {
            yield return null;
            TryAutoBootstrapStandaloneStageInEditor();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.f1Key.wasPressedThisFrame || keyboard.hKey.wasPressedThisFrame)
            {
                _showHelp = !_showHelp;
                return;
            }

            if (_helpHideTimer > 0f)
                _helpHideTimer -= Time.unscaledDeltaTime;

            if (keyboard.rKey.wasPressedThisFrame && !keyboard.shiftKey.isPressed)
            {
                ReloadActiveScene();
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                var active = SceneManager.GetActiveScene().name;
                if (active != AegisPcControls.BriefingSceneName)
                    SceneManager.LoadScene(AegisPcControls.BriefingSceneName);
                return;
            }

            if (keyboard.nKey.wasPressedThisFrame)
                TrySkipCurrentStage();
        }

        void TrySkipCurrentStage()
        {
            foreach (var stageRoot in FindObjectsByType<StageRoot>(FindObjectsInactive.Exclude))
            {
                if (!stageRoot.isActiveAndEnabled)
                    continue;

                stageRoot.RequestDebugComplete();
                return;
            }

            var stageManager = Object.FindAnyObjectByType<StageManager>();
            if (stageManager != null)
                stageManager.NotifyStageComplete();
        }

        void ReloadActiveScene()
        {
            if (_reloading)
                return;

            _reloading = true;
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0)
                SceneManager.LoadScene(scene.buildIndex);
            else
                SceneManager.LoadScene(scene.name);
        }

        void TryAutoBootstrapStandaloneStageInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isEditor || Application.isBatchMode)
                return;

            if (Object.FindAnyObjectByType<AegisMissionController>() != null)
                return;

            var sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.StartsWith("Stage"))
                return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Missions/Prefabs/AegisMission.prefab");
            if (prefab == null)
            {
                Debug.LogWarning("[AegisPlayModeServices] AegisMission prefab not found; open stage via AegisMissionFull or run Aegis → Ensure PC Play Setup In All Scenes.");
                return;
            }

            var missionGo = (GameObject)Object.Instantiate(prefab);
            missionGo.name = "AegisMission";

            var stageManager = missionGo.GetComponent<StageManager>();
            if (stageManager != null)
                ConfigureStageManagerForScene(stageManager, sceneName);

            if (Object.FindAnyObjectByType<AegisMissionBootstrap>() == null)
            {
                var bootstrapGo = new GameObject("AegisMissionBootstrap");
                var debugInput = bootstrapGo.AddComponent<DebugMissionInput>();
                var bootstrap = bootstrapGo.AddComponent<AegisMissionBootstrap>();
                bootstrap.Configure(missionGo.GetComponent<AegisMissionController>()!, debugInput);
            }

            Debug.Log($"[AegisPlayModeServices] Auto-bootstrapped mission shell for standalone stage '{sceneName}'.");
#endif
        }

#if UNITY_EDITOR
        static void ConfigureStageManagerForScene(StageManager stageManager, string sceneName)
        {
            var so = new SerializedObject(stageManager);
            var addresses = so.FindProperty("stageSceneAddresses");
            var editorPaths = so.FindProperty("editorScenePaths");

            string[] addrs;
            string[] paths;
            switch (sceneName)
            {
                case "Stage2_Lab":
                    addrs = new[] { "stage/2_lab" };
                    paths = new[] { "Assets/Scenes/Stages/Stage2_Lab.unity" };
                    break;
                case "Stage3_Datacenter":
                    addrs = new[] { "stage/3_datacenter" };
                    paths = new[] { "Assets/Scenes/Stages/Stage3_Datacenter.unity" };
                    break;
                case "Stage4_Core":
                    addrs = new[] { "stage/4_core" };
                    paths = new[] { "Assets/Scenes/Stages/Stage4_Core.unity" };
                    break;
                default:
                    addrs = StageSceneAddresses.Stage1Only;
                    paths = StageSceneAddresses.Stage1OnlyEditorPaths;
                    break;
            }

            addresses.arraySize = addrs.Length;
            for (var i = 0; i < addrs.Length; i++)
                addresses.GetArrayElementAtIndex(i).stringValue = addrs[i];

            editorPaths.arraySize = paths.Length;
            for (var i = 0; i < paths.Length; i++)
                editorPaths.GetArrayElementAtIndex(i).stringValue = paths[i];

            so.ApplyModifiedPropertiesWithoutUndo();
        }
#endif

        void OnGUI()
        {
            if (!_showHelp && _helpHideTimer <= 0f)
                return;

            var sceneName = SceneManager.GetActiveScene().name;
            var lines = BuildHelpLines(sceneName);
            var width = 420f;
            var height = 16f + lines.Length * 18f;
            var rect = new Rect(12f, Screen.height - height - 12f, width, height);

            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = Color.white;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = false,
                richText = false
            };

            var y = rect.y + 6f;
            foreach (var line in lines)
            {
                GUI.Label(new Rect(rect.x + 10f, y, rect.width - 20f, 18f), line, style);
                y += 18f;
            }

            if (_helpHideTimer > 0f)
            {
                GUI.Label(new Rect(rect.x + 10f, rect.y - 18f, rect.width, 16f),
                    "PC Controls (F1/H: toggle)", style);
            }
        }

        static string[] BuildHelpLines(string sceneName)
        {
            if (sceneName == AegisPcControls.BriefingSceneName)
            {
                return new[]
                {
                    $"{AegisPcControls.Actions.Fire}: —",
                    $"{AegisPcControls.Actions.SkipBriefingText}: {AegisPcControls.Keys.SkipBriefingText}",
                    $"{AegisPcControls.Actions.AcceptMission}: {AegisPcControls.Keys.AcceptMission}",
                    $"{AegisPcControls.Actions.RejectBriefing}: {AegisPcControls.Keys.RejectBriefing}",
                    $"{AegisPcControls.Actions.PathPreviewPlay}: {AegisPcControls.Keys.PathPreviewPlay}",
                    $"{AegisPcControls.Actions.PathPreviewReset}: {AegisPcControls.Keys.PathPreviewReset}",
                    $"{AegisPcControls.Actions.ReloadScene}: {AegisPcControls.Keys.ReloadScene}",
                };
            }

            if (sceneName.StartsWith("Stage") || sceneName == "AegisMissionFull")
            {
                var lines = new System.Collections.Generic.List<string>
                {
                    $"{AegisPcControls.Actions.Fire}: {AegisPcControls.Keys.Fire}",
                    $"{AegisPcControls.Actions.SkipStage}: {AegisPcControls.Keys.SkipStage}",
                    $"{AegisPcControls.Actions.ReturnToBriefing}: {AegisPcControls.Keys.ReturnToBriefing}",
                    $"{AegisPcControls.Actions.ReloadScene}: {AegisPcControls.Keys.ReloadScene}",
                    $"{AegisPcControls.Actions.ToggleHelp}: {AegisPcControls.Keys.ToggleHelp}",
                };

                if (sceneName.Contains("Stage1") || sceneName == "AegisMissionFull")
                {
                    lines.Add($"{AegisPcControls.Actions.JumpCut}: {AegisPcControls.Keys.JumpCut}");
                    lines.Add($"{AegisPcControls.Actions.PrevCut}/{AegisPcControls.Actions.NextCut}: {AegisPcControls.Keys.PrevCut} / {AegisPcControls.Keys.NextCut}");
                    lines.Add($"{AegisPcControls.Actions.ToggleTimeline}: {AegisPcControls.Keys.ToggleTimeline}");
                }

                return lines.ToArray();
            }

            return new[]
            {
                $"{AegisPcControls.Actions.ToggleHelp}: {AegisPcControls.Keys.ToggleHelp}",
                $"{AegisPcControls.Actions.ReloadScene}: {AegisPcControls.Keys.ReloadScene}",
                $"{AegisPcControls.Actions.ReturnToBriefing}: {AegisPcControls.Keys.ReturnToBriefing}",
            };
        }
    }
}
