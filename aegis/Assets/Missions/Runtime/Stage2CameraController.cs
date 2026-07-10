#nullable enable
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

namespace PinkSoft.Aegis.Missions
{
    public sealed class Stage2CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AegisMissionController? missionController;
        [SerializeField] private CinemachineCamera[] virtualCameras = new CinemachineCamera[7];

        [Header("Speed Settings (for testing)")]
        [SerializeField] private float timeScale = 1f;

        private int _currentVcamIndex = -1;
        private float _customElapsed;
        private bool _useCustomTimeline;

        // cut_timeline.md Stage2 타임코드 → 초
        private readonly float[] _startTimes =
        {
            0f,     // 2-1 Corridor
            90f,    // 2-2 Incubation
            180f,   // 2-3 Lab Zone
            290f,   // 2-4 Isolated (transition 270s)
            380f,   // 2-5 Data Storage
            470f,   // 2-6 Shadow
            580f    // 2-7 Boss RX-7 (transition 560s)
        };

        private readonly string[] _cameraNames =
        {
            "Vcam_2_1_Corridor",
            "Vcam_2_2_Incubation",
            "Vcam_2_3_LabZone",
            "Vcam_2_4_Isolated",
            "Vcam_2_5_DataStorage",
            "Vcam_2_6_Shadow",
            "Vcam_2_7_Boss"
        };

        private void Awake()
        {
            if (missionController == null)
                missionController = FindAnyObjectByType<AegisMissionController>();

            SetupVirtualCameras();
        }

        private void SetupVirtualCameras()
        {
            for (int i = 0; i < 7; i++)
            {
                var createdNew = false;

                if (virtualCameras[i] == null)
                {
                    var childTrans = transform.Find(_cameraNames[i]);
                    if (childTrans != null)
                        virtualCameras[i] = childTrans.GetComponent<CinemachineCamera>();
                    else
                    {
                        var camGo = new GameObject(_cameraNames[i]);
                        camGo.transform.SetParent(transform);
                        virtualCameras[i] = camGo.AddComponent<CinemachineCamera>();
                        createdNew = true;
                    }
                }

                if (virtualCameras[i] == null)
                    continue;

                if (createdNew)
                    ConfigureCameraPreset(i, virtualCameras[i]);
                else
                    ResetCameraPriority(virtualCameras[i]);
            }
        }

        static void ResetCameraPriority(CinemachineCamera vcam)
        {
            var priority = vcam.Priority;
            priority.Value = 10;
            vcam.Priority = priority;
        }

        private void ConfigureCameraPreset(int index, CinemachineCamera vcam)
        {
            var t = vcam.transform;
            switch (index)
            {
                case 0:
                    t.localPosition = new Vector3(0f, 1.5f, -20f);
                    t.localEulerAngles = new Vector3(4f, 0f, 0f);
                    break;
                case 1:
                    t.localPosition = new Vector3(-3f, 1.2f, -6f);
                    t.localEulerAngles = new Vector3(8f, 18f, 0f);
                    break;
                case 2:
                    t.localPosition = new Vector3(0f, 2.2f, 4f);
                    t.localEulerAngles = new Vector3(12f, 0f, 0f);
                    break;
                case 3:
                    t.localPosition = new Vector3(-2f, 1.8f, 14f);
                    t.localEulerAngles = new Vector3(6f, 14f, 0f);
                    break;
                case 4:
                    t.localPosition = new Vector3(0f, 2.5f, 24f);
                    t.localEulerAngles = new Vector3(10f, 0f, 0f);
                    break;
                case 5:
                    t.localPosition = new Vector3(4f, 1.4f, 34f);
                    t.localEulerAngles = new Vector3(-5f, -22f, 0f);
                    break;
                case 6:
                    t.localPosition = new Vector3(0f, 1.2f, 40f);
                    t.localEulerAngles = new Vector3(8f, 0f, 0f);
                    break;
            }

            ResetCameraPriority(vcam);
        }

        static float EaseInOut(float t) => t * t * (3f - 2f * t);
        static float EaseOutCubic(float t) { var inv = 1f - t; return 1f - inv * inv * inv; }

        private void Update()
        {
            float elapsed = GetElapsedTime();
            HandleDebugInputs();

            int targetIndex = 0;
            for (int i = 0; i < _startTimes.Length; i++)
            {
                if (elapsed >= _startTimes[i])
                    targetIndex = i;
            }

            if (targetIndex != _currentVcamIndex)
                SetActiveCamera(targetIndex);

            AnimateCameraMovement(targetIndex, elapsed);
        }

        private float GetElapsedTime()
        {
            if (_useCustomTimeline)
            {
                _customElapsed += Time.deltaTime * timeScale;
                return _customElapsed;
            }

            if (missionController != null)
                return missionController.ElapsedTime * timeScale;

            return Time.time * timeScale;
        }

        private void SetActiveCamera(int index)
        {
            _currentVcamIndex = index;
            for (int i = 0; i < virtualCameras.Length; i++)
            {
                if (virtualCameras[i] == null) continue;
                var priority = virtualCameras[i].Priority;
                priority.Value = i == index ? 15 : 10;
                virtualCameras[i].Priority = priority;
            }
            Debug.Log($"[Stage2CameraController] Switched to {_cameraNames[index]} at {GetElapsedTime():F2}s");
        }

        private void AnimateCameraMovement(int index, float elapsed)
        {
            if (index < 0 || index >= virtualCameras.Length || virtualCameras[index] == null) return;

            var t = virtualCameras[index].transform;
            float phaseStart = _startTimes[index];
            float phaseElapsed = elapsed - phaseStart;
            float phaseDuration = index + 1 < _startTimes.Length ? _startTimes[index + 1] - phaseStart : 320f;
            float pct = EaseInOut(Mathf.Clamp01(phaseElapsed / phaseDuration));

            switch (index)
            {
                case 0: // 손전등 시점 — 천천히 코너 돌며 전진
                    {
                        float creep = EaseOutCubic(Mathf.Clamp01(phaseElapsed / 20f));
                        t.localPosition = Vector3.Lerp(
                            new Vector3(0f, 1.5f, -20f),
                            new Vector3(0.5f, 1.45f, -14f),
                            creep);
                        var flicker = Mathf.Sin(phaseElapsed * 3.5f) * 0.08f;
                        t.localEulerAngles = new Vector3(4f + flicker, Mathf.Sin(pct * Mathf.PI) * 8f, flicker * 0.5f);
                    }
                    break;

                case 1: // 배양 탱크 홀 — 좌우 탱크 사이 클로즈업
                    t.localPosition = Vector3.Lerp(
                        new Vector3(-3f, 1.2f, -6f),
                        new Vector3(2f, 1.5f, -2f),
                        pct);
                    t.localEulerAngles = new Vector3(8f + pct * 4f, Mathf.Lerp(18f, -12f, pct), 0f);
                    break;

                case 2: // 실험실 2구역 — 방패병 향해 줌인
                    t.localPosition = Vector3.Lerp(
                        new Vector3(0f, 2.2f, 4f),
                        new Vector3(0f, 1.6f, 10f),
                        pct);
                    t.localEulerAngles = new Vector3(Mathf.Lerp(12f, 6f, pct), 0f, 0f);
                    break;

                case 3: // 고립 복도 — 지그재그 전진
                    {
                        float weave = Mathf.Sin(pct * Mathf.PI * 2.5f) * 2f;
                        t.localPosition = new Vector3(-2f + weave, 1.8f, Mathf.Lerp(14f, 22f, pct));
                        t.localEulerAngles = new Vector3(6f, 14f - weave * 3f, 0f);
                    }
                    break;

                case 4: // 데이터 보관실 — 서버랙 사이 팬
                    {
                        float pan = Mathf.Lerp(-25f, 25f, pct);
                        t.localPosition = new Vector3(Mathf.Lerp(-3f, 3f, pct), 2.5f, Mathf.Lerp(24f, 30f, pct));
                        t.localEulerAngles = new Vector3(10f, pan, 0f);
                    }
                    break;

                case 5: // 벽 그림자 — 저각 스윕
                    t.localPosition = Vector3.Lerp(
                        new Vector3(4f, 1.4f, 34f),
                        new Vector3(-3f, 1.2f, 40f),
                        pct);
                    t.localEulerAngles = new Vector3(-5f + pct * 3f, Mathf.Lerp(-22f, 15f, pct), 0f);
                    break;

                case 6: // RX-7 보스 — 다각도 추적 + 압박 돌진
                    {
                        float orbit = Mathf.Sin(pct * Mathf.PI * 2f) * 12f;
                        t.localPosition = new Vector3(
                            Mathf.Sin(orbit * Mathf.Deg2Rad) * 2.5f,
                            1.2f + pct * 0.4f,
                            Mathf.Lerp(40f, 44f, pct));
                        t.localEulerAngles = new Vector3(8f, orbit, 0f);
                    }
                    break;
            }
        }

        private void HandleDebugInputs()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.tKey.wasPressedThisFrame)
            {
                _useCustomTimeline = !_useCustomTimeline;
                if (_useCustomTimeline) _customElapsed = 0f;
            }

            for (int i = 0; i < 7; i++)
            {
                var key = i switch
                {
                    0 => keyboard.digit1Key,
                    1 => keyboard.digit2Key,
                    2 => keyboard.digit3Key,
                    3 => keyboard.digit4Key,
                    4 => keyboard.digit5Key,
                    5 => keyboard.digit6Key,
                    6 => keyboard.digit7Key,
                    _ => null
                };

                if (key != null && key.wasPressedThisFrame)
                {
                    _useCustomTimeline = true;
                    _customElapsed = _startTimes[i];
                    SetActiveCamera(i);
                }
            }

            if (keyboard.leftBracketKey.wasPressedThisFrame)
            {
                int prev = Mathf.Max(0, _currentVcamIndex - 1);
                _useCustomTimeline = true;
                _customElapsed = _startTimes[prev];
                SetActiveCamera(prev);
            }

            if (keyboard.rightBracketKey.wasPressedThisFrame)
            {
                int next = Mathf.Min(6, _currentVcamIndex + 1);
                _useCustomTimeline = true;
                _customElapsed = _startTimes[next];
                SetActiveCamera(next);
            }
        }
    }
}
