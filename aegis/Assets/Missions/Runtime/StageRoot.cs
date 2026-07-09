#nullable enable
using PinkSoft.MissionSDK;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PinkSoft.Aegis.Missions
{
    /// <summary>스테이지 씬 루트 — additive 로드 후 StageManager에 완료를 알립니다.</summary>
    public sealed class StageRoot : MonoBehaviour
    {
        [SerializeField] StageManager? stageManager;
        [SerializeField] bool autoCompleteOnAllTargetsDisabled;
        [SerializeField] Key debugCompleteKey = Key.N;

        void Awake()
        {
            if (stageManager == null)
                stageManager = FindAnyObjectByType<StageManager>();
        }

        void Update()
        {
            if (stageManager == null || !isActiveAndEnabled)
                return;

            if (debugCompleteKey != Key.None)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null && keyboard[debugCompleteKey].wasPressedThisFrame)
                    stageManager.NotifyStageComplete();
            }

            if (!autoCompleteOnAllTargetsDisabled)
                return;

            var targets = GetComponentsInChildren<Collider>(true);
            if (targets.Length == 0)
                return;

            foreach (var target in targets)
            {
                if (target.enabled && target.gameObject.activeInHierarchy)
                    return;
            }

            stageManager.NotifyStageComplete();
        }
    }
}
