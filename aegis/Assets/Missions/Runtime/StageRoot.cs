using PinkSoft.MissionSDK;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PinkSoft.Aegis.Missions
{
    /// <summary>스테이지 루트 — 테스트용 타겟 적중 또는 완료 트리거.</summary>
    public sealed class StageRoot : MonoBehaviour
    {
        [SerializeField] StageManager stageManager = null!;
        [SerializeField] bool autoCompleteOnAllTargetsDisabled;
        [SerializeField] Key debugCompleteKey = Key.N;

        void Awake()
        {
            if (stageManager == null)
                stageManager = GetComponentInParent<StageManager>(true);
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
