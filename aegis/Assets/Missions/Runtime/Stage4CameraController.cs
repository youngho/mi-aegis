#nullable enable
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

namespace PinkSoft.Aegis.Missions
{
    public sealed class Stage4CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AegisMissionController? missionController;
        [SerializeField] private CinemachineCamera[] virtualCameras = new CinemachineCamera[7];

        [Header("Speed Settings (for testing)")]
        [SerializeField] private float timeScale = 1f;

        private int _currentVcamIndex = -1;
        private float _customElapsed;
        private bool _useCustomTimeline;

        // cut_timeline.md Stage4 타임코드 → 초
        private readonly float[] _startTimes =
        {
            0f,     // 4-1 Elevator Arrival (0:00)
            90f,    // 4-2 CEO Office Inside (1:30)
            200f,   // 4-3 Balcony Night View (3:20, transition 180s)
            290f,   // 4-4 Security Corridor (4:50)
            380f,   // 4-5 Mainframe Access (6:20)
            490f,   // 4-6 Boss P1 (8:10, transition 470s)
            780f    // 4-7 Boss P2 (13:00, transition 750s)
        };

        private readonly string[] _cameraNames =
        {
            "Vcam_4_1_Entrance",
            "Vcam_4_2_Office",
            "Vcam_4_3_Balcony",
            "Vcam_4_4_Corridor",
            "Vcam_4_5_MainframeAccess",
            "Vcam_4_6_Boss_Alex",
            "Vcam_4_7_Boss_AegisCore"
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
                    t.localPosition = new Vector3(0f, 1.6f, -26f);
                    t.localEulerAngles = new Vector3(5f, 0f, 0f);
                    break;
                case 1:
                    t.localPosition = new Vector3(-2f, 1.4f, -12f);
                    t.localEulerAngles = new Vector3(3f, 12f, 0f);
                    break;
                case 2:
                    t.localPosition = new Vector3(0f, 2f, 2f);
                    t.localEulerAngles = new Vector3(10f, 0f, 0f);
                    break;
                case 3:
                    t.localPosition = new Vector3(0f, 2.4f, 16f);
                    t.localEulerAngles = new Vector3(14f, 0f, 0f);
                    break;
                case 4:
                    t.localPosition = new Vector3(0f, 1.5f, 30f);
                    t.localEulerAngles = new Vector3(4f, 0f, 0f);
                    break;
                case 5:
                    t.localPosition = new Vector3(0f, 1.8f, 44f);
                    t.localEulerAngles = new Vector3(2f, 0f, 0f);
                    break;
                case 6:
                    t.localPosition = new Vector3(0f, 1.8f, 58f);
                    t.localEulerAngles = new Vector3(12f, 0f, 0f);
                    break;
            }

            ResetCameraPriority(vcam);
        }

        static float EaseInOut(float t) => t * t * (3f - 2f * t);

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
            Debug.Log($"[Stage4CameraController] Switched to {_cameraNames[index]} at {GetElapsedTime():F2}s");
        }

        private void AnimateCameraMovement(int index, float elapsed)
        {
            if (index < 0 || index >= virtualCameras.Length || virtualCameras[index] == null) return;

            var t = virtualCameras[index].transform;
            float phaseStart = _startTimes[index];
            float phaseElapsed = elapsed - phaseStart;
            float phaseDuration = index + 1 < _startTimes.Length ? _startTimes[index + 1] - phaseStart : 420f;
            float pct = EaseInOut(Mathf.Clamp01(phaseElapsed / phaseDuration));

            switch (index)
            {
                case 0: // 4-1 Elevator Lobby (야경). Slow creep forward.
                    {
                        float creep = Mathf.Clamp01(phaseElapsed / 20f);
                        t.localPosition = Vector3.Lerp(
                            new Vector3(0f, 1.6f, -26f),
                            new Vector3(0f, 1.55f, -20f),
                            creep);
                        t.localEulerAngles = new Vector3(5f, 0f, 0f);
                    }
                    break;

                case 1: // 4-2 Penthouse Office (파괴 시퀀스). 180도 회전하며 거실 가구 사이를 구르는 연출.
                    t.localPosition = Vector3.Lerp(
                        new Vector3(-2f, 1.4f, -12f),
                        new Vector3(2f, 1.2f, -6f),
                        pct);
                    t.localEulerAngles = new Vector3(3f, Mathf.Lerp(12f, 192f, pct), 0f);
                    break;

                case 2: // 4-3 Balcony Night View (야경 연출).
                    t.localPosition = Vector3.Lerp(
                        new Vector3(0f, 2f, 2f),
                        new Vector3(0f, 1.8f, 8f),
                        pct);
                    t.localEulerAngles = new Vector3(10f, Mathf.Sin(phaseElapsed * 0.1f) * 15f, 0f);
                    break;

                case 3: // 4-4 Security Corridor (불릿 타임). 보안 터렛 피하기, 미세한 떨림 연출.
                    {
                        float shake = Mathf.Sin(phaseElapsed * 15f) * 0.1f;
                        t.localPosition = Vector3.Lerp(
                            new Vector3(0f, 2.4f, 16f),
                            new Vector3(shake, 1.5f, 22f),
                            pct);
                        t.localEulerAngles = new Vector3(14f, shake * 2f, 0f);
                    }
                    break;

                case 4: // 4-5 Mainframe Access.
                    t.localPosition = Vector3.Lerp(
                        new Vector3(0f, 1.5f, 30f),
                        new Vector3(0f, 1.3f, 36f),
                        pct);
                    t.localEulerAngles = new Vector3(4f, 0f, 0f);
                    break;

                case 5: // 4-6 Boss Alex Phase 1. Exo-suit 기동 사격 추적 연출.
                    {
                        float sway = Mathf.Sin(phaseElapsed * 0.8f) * 2.5f;
                        t.localPosition = new Vector3(sway, 1.8f + Mathf.Cos(phaseElapsed * 0.5f) * 0.2f, Mathf.Lerp(44f, 48f, pct));
                        t.localEulerAngles = new Vector3(2f, sway * -2f, 0f);
                    }
                    break;

                case 6: // 4-7 Boss Aegis Core Phase 2. Core 주위 360도 공전.
                    {
                        float orbit = pct * 360f;
                        t.localPosition = new Vector3(
                            Mathf.Sin(orbit * Mathf.Deg2Rad) * 4f,
                            2f + Mathf.Sin(pct * Mathf.PI) * 0.4f,
                            Mathf.Lerp(58f, 65f, pct));
                        t.localEulerAngles = new Vector3(12f, orbit, 0f);
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
