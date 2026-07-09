using PinkSoft.MissionSDK;
using UnityEngine;

namespace PinkSoft.Aegis.Missions
{
    /// <summary>mi-aegis 단독 테스트 — DebugMissionInput으로 AegisMissionController를 구동.</summary>
    public sealed class AegisMissionBootstrap : MonoBehaviour
    {
        [SerializeField] AegisMissionController missionController = null!;
        [SerializeField] DebugMissionInput debugInput = null!;

        void Awake()
        {
            if (missionController == null)
                missionController = FindAnyObjectByType<AegisMissionController>();
            if (debugInput == null)
                debugInput = GetComponent<DebugMissionInput>();
        }

        public void Configure(AegisMissionController controller, DebugMissionInput input)
        {
            missionController = controller;
            debugInput = input;
        }

        void Start()
        {
            if (missionController == null || debugInput == null)
            {
                Debug.LogError("[AegisMissionBootstrap] missionController or debugInput is missing.");
                return;
            }

            var context = new MissionContext
            {
                input = debugInput,
                config = new MissionConfig
                {
                    difficultyLevel = 2,
                    timeLimitSeconds = 3600,
                    targetScore = 10000
                }
            };

            missionController.InitializeMission(new RuntimeUserData
            {
                userId = "dev",
                nickname = "Developer"
            }, context);

            missionController.OnMissionEnded += (success, result) =>
                Debug.Log($"[AegisMissionBootstrap] Mission ended success={success} score={result.finalScore}");
        }

        void OnDestroy()
        {
            if (missionController != null)
                missionController.Shutdown();
        }
    }
}
