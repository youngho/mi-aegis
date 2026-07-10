#nullable enable
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

namespace PinkSoft.Aegis.Missions
{
    public sealed class Stage1CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AegisMissionController? missionController;
        [SerializeField] private CinemachineCamera[] virtualCameras = new CinemachineCamera[7];

        [Header("Speed Settings (for testing)")]
        [SerializeField] private float timeScale = 1f;

        private int _currentVcamIndex = -1;
        private float _customElapsed = 0f;
        private bool _useCustomTimeline = false;

        // Start times of each phase in seconds (based on README.md)
        // Cut 1-1: 0:00 (0s)
        // Cut 1-2: 1:20 (80s)
        // Transition 1 (Shutter 1/2 open): 2:40 (160s) -> triggers Vcam 3
        // Cut 1-3: 3:00 (180s)
        // Cut 1-4: 4:20 (260s)
        // Transition 2 (Shutter full open): 5:40 (340s) -> triggers Vcam 5
        // Cut 1-5: 6:00 (360s)
        // Cut 1-6: 7:20 (440s)
        // Transition 3 (APC entrance): 8:40 (520s) -> triggers Vcam 7
        // Cut 1-7 Boss: 9:00 (540s)
        private readonly float[] _startTimes = new float[]
        {
            0f,     // Cut 1-1
            80f,    // Cut 1-2
            160f,   // Cut 1-3 (Starts transition at 160s)
            260f,   // Cut 1-4
            340f,   // Cut 1-5 (Starts transition at 340s)
            440f,   // Cut 1-6
            520f    // Cut 1-7 Boss (Starts transition at 520s)
        };

        private readonly string[] _cameraNames = new string[]
        {
            "Vcam_1_1_Entrance",
            "Vcam_1_2_Reception",
            "Vcam_1_3_Balcony",
            "Vcam_1_4_Corridor",
            "Vcam_1_5_ElevatorLobby",
            "Vcam_1_6_ParkingLot",
            "Vcam_1_7_Boss"
        };

        private void Awake()
        {
            if (missionController == null)
            {
                missionController = FindAnyObjectByType<AegisMissionController>();
            }

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
                    {
                        virtualCameras[i] = childTrans.GetComponent<CinemachineCamera>();
                    }
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
                case 0: // 1-1 Entrance — 대각 진입 와이드
                    t.localPosition = new Vector3(-4.0f, 5.2f, -42.0f);
                    t.localEulerAngles = new Vector3(8.0f, 6.0f, 0.0f);
                    break;
                case 1: // 1-2 Reception — 데스크 저공 슬라이드
                    t.localPosition = new Vector3(8.0f, 2.8f, -22.0f);
                    t.localEulerAngles = new Vector3(12.0f, -18.0f, 0.0f);
                    break;
                case 2: // 1-3 Balcony — 좌측 앙각
                    t.localPosition = new Vector3(-12.0f, 2.2f, -6.0f);
                    t.localEulerAngles = new Vector3(-18.0f, 22.0f, 0.0f);
                    break;
                case 3: // 1-4 Corridor — 기둥 사이 지그재그 시작
                    t.localPosition = new Vector3(-8.0f, 6.2f, -24.0f);
                    t.localEulerAngles = new Vector3(6.0f, 12.0f, 0.0f);
                    break;
                case 4: // 1-5 Elevator Lobby — 좌측 팬 시작
                    t.localPosition = new Vector3(-16.0f, 6.8f, 18.0f);
                    t.localEulerAngles = new Vector3(5.0f, 35.0f, 0.0f);
                    break;
                case 5: // 1-6 Parking Lot — 고각 하강 시작
                    t.localPosition = new Vector3(6.0f, 11.0f, 22.0f);
                    t.localEulerAngles = new Vector3(18.0f, -8.0f, 0.0f);
                    break;
                case 6: // 1-7 Boss — APC 로우 앵글
                    t.localPosition = new Vector3(0.0f, 2.5f, 4.0f);
                    t.localEulerAngles = new Vector3(4.0f, 0.0f, 0.0f);
                    break;
            }

            ResetCameraPriority(vcam);
        }

        static float EaseInOut(float t) => t * t * (3f - 2f * t);

        static float EaseOutCubic(float t)
        {
            var inv = 1f - t;
            return 1f - inv * inv * inv;
        }

        private void Update()
        {
            float elapsed = GetElapsedTime();

            // Handle manual overrides for debugging
            HandleDebugInputs();

            // Determine active Vcam based on timeline
            int targetIndex = 0;
            for (int i = 0; i < _startTimes.Length; i++)
            {
                if (elapsed >= _startTimes[i])
                {
                    targetIndex = i;
                }
            }

            if (targetIndex != _currentVcamIndex)
            {
                SetActiveCamera(targetIndex);
            }

            // Animate moving cameras based on elapsed time within phase
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
            {
                return missionController.ElapsedTime * timeScale;
            }

            return Time.time * timeScale;
        }

        private void SetActiveCamera(int index)
        {
            _currentVcamIndex = index;
            for (int i = 0; i < virtualCameras.Length; i++)
            {
                if (virtualCameras[i] != null)
                {
                    var priority = virtualCameras[i].Priority;
                    priority.Value = (i == index) ? 15 : 10;
                    virtualCameras[i].Priority = priority;
                }
            }
            Debug.Log($"[Stage1CameraController] Switched to camera: {_cameraNames[index]} at elapsed={GetElapsedTime():F2}s");
        }

        private void AnimateCameraMovement(int index, float elapsed)
        {
            if (index < 0 || index >= virtualCameras.Length || virtualCameras[index] == null) return;

            var vcam = virtualCameras[index];
            var t = vcam.transform;
            float phaseStart = _startTimes[index];
            float phaseElapsed = elapsed - phaseStart;
            float phaseDuration = index + 1 < _startTimes.Length
                ? _startTimes[index + 1] - phaseStart
                : 180f;
            float pct = EaseInOut(Mathf.Clamp01(phaseElapsed / phaseDuration));

            switch (index)
            {
                case 0: // 1-1 Entrance — 폭발 직후 급진입 + 미세 흔들림
                    {
                        float rush = EaseOutCubic(Mathf.Clamp01(phaseElapsed / 12f));
                        t.localPosition = Vector3.Lerp(
                            new Vector3(-4.0f, 5.2f, -42.0f),
                            new Vector3(-1.2f, 4.2f, -32.0f),
                            rush);
                        var sway = Mathf.Sin(phaseElapsed * 1.8f) * 0.35f;
                        t.localEulerAngles = new Vector3(8f + sway, 6f - rush * 4f, sway * 0.6f);
                    }
                    break;

                case 1: // 1-2 Reception — 데스크 위 저공 슬라이드 + 줌인
                    {
                        t.localPosition = Vector3.Lerp(
                            new Vector3(8.0f, 2.8f, -22.0f),
                            new Vector3(2.4f, 2.2f, -17.0f),
                            pct);
                        t.localEulerAngles = Vector3.Lerp(
                            new Vector3(12f, -18f, 0f),
                            new Vector3(8f, -6f, 0f),
                            pct);
                    }
                    break;

                case 2: // 1-3 Balcony — 좌→우 스윕 앙각
                    {
                        float sweep = Mathf.Sin(pct * Mathf.PI);
                        t.localPosition = new Vector3(
                            Mathf.Lerp(-12f, 10f, pct),
                            2.2f + sweep * 0.8f,
                            Mathf.Lerp(-6f, -3f, pct));
                        t.localEulerAngles = new Vector3(
                            -18f + sweep * 4f,
                            Mathf.Lerp(22f, -16f, pct),
                            0f);
                    }
                    break;

                case 3: // 1-4 Corridor — 기둥 사이 지그재그 전진
                    {
                        float forward = Mathf.Lerp(-24f, 12f, pct);
                        float weave = Mathf.Sin(pct * Mathf.PI * 3f) * 6f;
                        t.localPosition = new Vector3(-8f + weave, 6.2f - pct * 1f, forward);
                        t.localEulerAngles = new Vector3(6f, 12f - weave * 2f, weave * 0.4f);
                    }
                    break;

                case 4: // 1-5 Elevator Lobby — 좌→우 팬 + 미세 상하 바운스
                    {
                        float pan = Mathf.Lerp(35f, -35f, pct);
                        float bob = Mathf.Sin(phaseElapsed * 0.9f) * 0.25f;
                        t.localPosition = new Vector3(
                            Mathf.Lerp(-16f, 16f, pct),
                            6.8f + bob * 0.4f,
                            Mathf.Lerp(18f, 24f, pct));
                        t.localEulerAngles = new Vector3(5f + bob * 2f, pan, 0f);
                    }
                    break;

                case 5: // 1-6 Parking Lot — 하강 + 전진 돌입
                    {
                        t.localPosition = Vector3.Lerp(
                            new Vector3(6f, 11f, 22f),
                            new Vector3(0f, 2.8f, 33f),
                            pct);
                        t.localEulerAngles = Vector3.Lerp(
                            new Vector3(18f, -8f, 0f),
                            new Vector3(6f, 0f, 0f),
                            pct);
                    }
                    break;

                case 6: // 1-7 Boss — APC 향해 압박 돌진
                    {
                        float push = EaseOutCubic(Mathf.Clamp01(phaseElapsed / 25f));
                        t.localPosition = Vector3.Lerp(
                            new Vector3(0f, 2.5f, 4f),
                            new Vector3(0f, 3.2f, 11f),
                            push);
                        var pulse = Mathf.Sin(phaseElapsed * 2.2f) * 0.15f;
                        t.localEulerAngles = new Vector3(4f + pulse, 0f, 0f);
                    }
                    break;
            }
        }

        private void HandleDebugInputs()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // T key toggles custom timeline for easy previewing/testing
            if (keyboard.tKey.wasPressedThisFrame)
            {
                _useCustomTimeline = !_useCustomTimeline;
                if (_useCustomTimeline)
                {
                    _customElapsed = 0f;
                }
                Debug.Log($"[Stage1CameraController] UseCustomTimeline={_useCustomTimeline}, timeScale={timeScale}");
            }

            // Alpha keys (1-7) jump directly to cuts
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
                    Debug.Log($"[Stage1CameraController] Debug Jump to Cut {i + 1} ({_cameraNames[i]}) at elapsed={_customElapsed}s");
                }
            }

            // [ and ] keys navigate backward and forward
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
