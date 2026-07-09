#nullable enable
using System;
using System.Collections;
using PinkSoft.MissionSDK;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace PinkSoft.Aegis.Missions
{
    /// <summary>스테이지 씬을 Addressables(additive)로 순차 로드합니다.</summary>
    public sealed class StageManager : MonoBehaviour
    {
        [SerializeField] string[] stageSceneAddresses = StageSceneAddresses.All;
        [SerializeField] bool useEditorScenePathsInPlayMode = true;

#if UNITY_EDITOR
        [SerializeField] string[] editorScenePaths = StageSceneAddresses.EditorPaths;
#endif

        int _currentIndex = -1;
        bool _advancing;
        bool _useAddressablesScene;
        bool _embeddedStageActive;
        Scene _loadedScene;
        SceneInstance _addressableScene;

        public event Action<int>? OnStageStarted;
        public event Action? OnAllStagesComplete;
        public event Action<ScoreEventType, string>? ReportEventRequested;

        public int CurrentStageIndex => _currentIndex;
        public int StageCount => stageSceneAddresses.Length;

        public void Begin()
        {
            _currentIndex = -1;
            StartCoroutine(AdvanceRoutine());
        }

        public void NotifyStageComplete()
        {
            ReportEvent(ScoreEventType.TimeBonus, $"stage{_currentIndex + 1}_clear");
            StartCoroutine(AdvanceRoutine());
        }

        IEnumerator AdvanceRoutine()
        {
            if (_advancing)
                yield break;

            _advancing = true;
            yield return UnloadCurrentScene();

            _currentIndex++;
            if (_currentIndex >= stageSceneAddresses.Length)
            {
                _advancing = false;
                OnAllStagesComplete?.Invoke();
                yield break;
            }

            var loaded = false;
            yield return LoadStageScene(_currentIndex, ok => loaded = ok);
            if (!loaded)
            {
                Debug.LogError($"[StageManager] Failed to load stage index={_currentIndex}.");
                _advancing = false;
                yield break;
            }

            OnStageStarted?.Invoke(_currentIndex);
            _advancing = false;
        }

        IEnumerator UnloadCurrentScene()
        {
            if (_embeddedStageActive)
            {
                _embeddedStageActive = false;
                _loadedScene = default;
                yield break;
            }

            if (_useAddressablesScene)
            {
                if (_addressableScene.Scene.isLoaded)
                {
                    var unload = Addressables.UnloadSceneAsync(_addressableScene);
                    yield return unload;
                }

                _useAddressablesScene = false;
                _addressableScene = default;
                _loadedScene = default;
                yield break;
            }

            if (!_loadedScene.IsValid() || !_loadedScene.isLoaded)
                yield break;

            var op = SceneManager.UnloadSceneAsync(_loadedScene);
            if (op != null)
                yield return op;

            _loadedScene = default;
        }

        IEnumerator LoadStageScene(int index, Action<bool> onDone)
        {
#if UNITY_EDITOR
            if (useEditorScenePathsInPlayMode && Application.isEditor && !Application.isBatchMode
                && editorScenePaths != null && index < editorScenePaths.Length)
            {
                var path = editorScenePaths[index];
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (IsStageOpenAsActiveScene(sceneName))
                    {
                        _loadedScene = SceneManager.GetSceneByName(sceneName);
                        _embeddedStageActive = true;
                        _useAddressablesScene = false;
                        onDone(true);
                        yield break;
                    }

                    var op = EditorSceneManager.LoadSceneAsyncInPlayMode(
                        path,
                        new LoadSceneParameters(LoadSceneMode.Additive));

                    if (op != null)
                        yield return op;
                    else
                        EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));

                    var loadedScene = SceneManager.GetSceneByName(sceneName);
                    if (loadedScene.isLoaded)
                    {
                        StripMissionShellObjects(loadedScene);
                        _loadedScene = loadedScene;
                        _useAddressablesScene = false;
                        onDone(true);
                        yield break;
                    }

                    Debug.LogError($"[StageManager] Editor load failed for '{path}'.");
                    onDone(false);
                    yield break;
                }
            }
#endif

            var address = stageSceneAddresses[index];
            var sceneNameFromAddress = GetSceneNameForIndex(index);
            if (!string.IsNullOrEmpty(sceneNameFromAddress) && IsStageOpenAsActiveScene(sceneNameFromAddress))
            {
                _loadedScene = SceneManager.GetSceneByName(sceneNameFromAddress);
                _embeddedStageActive = true;
                _useAddressablesScene = false;
                onDone(true);
                yield break;
            }

            var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive, activateOnLoad: true);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[StageManager] Addressables load failed for '{address}'.");
                onDone(false);
                yield break;
            }

            _addressableScene = handle.Result;
            _loadedScene = _addressableScene.Scene;
            StripMissionShellObjects(_loadedScene);
            _useAddressablesScene = true;
            onDone(true);
        }

        /// <summary>스테이지 씬에 포함된 기본 Main Camera·조명·미션 셸은 미션 셸 씬 것만 사용합니다.</summary>
        static void StripMissionShellObjects(Scene stageScene)
        {
            if (!stageScene.IsValid())
                return;

            foreach (var root in stageScene.GetRootGameObjects())
            {
                switch (root.name)
                {
                    case "Main Camera":
                    case "Directional Light":
                    case "AegisMission":
                    case "AegisMissionBootstrap":
                        UnityEngine.Object.Destroy(root);
                        break;
                }
            }
        }

        void OnDestroy()
        {
            if (!Application.isPlaying)
                return;

            if (_useAddressablesScene && _addressableScene.Scene.isLoaded)
                Addressables.UnloadSceneAsync(_addressableScene);
            else if (_loadedScene.IsValid() && _loadedScene.isLoaded)
                SceneManager.UnloadSceneAsync(_loadedScene);
        }

        void ReportEvent(ScoreEventType eventType, string targetId) =>
            ReportEventRequested?.Invoke(eventType, targetId);

        static bool IsStageOpenAsActiveScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded && scene == SceneManager.GetActiveScene();
        }

        string? GetSceneNameForIndex(int index)
        {
#if UNITY_EDITOR
            if (editorScenePaths != null && index < editorScenePaths.Length
                && !string.IsNullOrEmpty(editorScenePaths[index]))
                return System.IO.Path.GetFileNameWithoutExtension(editorScenePaths[index]);
#endif
            return index switch
            {
                0 => "Stage1_Lobby",
                1 => "Stage2_Lab",
                2 => "Stage3_Datacenter",
                3 => "Stage4_Core",
                _ => null
            };
        }
    }
}
