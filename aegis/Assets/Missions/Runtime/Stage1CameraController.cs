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
                case 0: // 1-1 Entrance
                    t.localPosition = new Vector3(0.0f, 2.1f, -20.0f);
                    t.localEulerAngles = new Vector3(10.0f, 0.0f, 0.0f);
                    break;
                case 1: // 1-2 Reception
                    t.localPosition = new Vector3(0.0f, 2.0f, -11.0f);
                    t.localEulerAngles = new Vector3(8.0f, 0.0f, 0.0f);
                    break;
                case 2: // 1-3 Balcony
                    t.localPosition = new Vector3(0.0f, 0.8f, -4.0f);
                    t.localEulerAngles = new Vector3(-25.0f, 0.0f, 0.0f); // Low angle looking up
                    break;
                case 3: // 1-4 Corridor (Start pos)
                    t.localPosition = new Vector3(0.0f, 2.0f, -10.0f);
                    t.localEulerAngles = new Vector3(6.0f, 0.0f, 0.0f);
                    break;
                case 4: // 1-5 Elevator Lobby
                    t.localPosition = new Vector3(0.0f, 2.0f, 10.0f);
                    t.localEulerAngles = new Vector3(5.0f, 0.0f, 0.0f);
                    break;
                case 5: // 1-6 Parking Lot (Start pos)
                    t.localPosition = new Vector3(0.0f, 3.5f, 14.0f);
                    t.localEulerAngles = new Vector3(12.0f, 0.0f, 0.0f);
                    break;
                case 6: // 1-7 Boss
                    t.localPosition = new Vector3(0.0f, 2.0f, 5.0f);
                    t.localEulerAngles = new Vector3(6.0f, 0.0f, 0.0f);
                    break;
            }

            // Set priority struct
            ResetCameraPriority(vcam);
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

            switch (index)
            {
                case 3: // 1-4 Corridor: Move forward from z=-10 to z=5 over 80s (4:20-5:40)
                    {
                        float duration = 80f;
                        float pct = Mathf.Clamp01(phaseElapsed / duration);
                        float startZ = -10.0f;
                        float endZ = 5.0f;
                        t.localPosition = new Vector3(0.0f, 2.0f, Mathf.Lerp(startZ, endZ, pct));
                    }
                    break;

                case 4: // 1-5 Elevator Lobby: Pan Y from -30 to 30 degrees (slow pan back and forth)
                    {
                        // Period of 8 seconds for full back-and-forth swing
                        float angle = Mathf.Sin(phaseElapsed * 0.25f) * 30.0f;
                        t.localEulerAngles = new Vector3(5.0f, angle, 0.0f);
                    }
                    break;

                case 5: // 1-6 Parking Lot: Descend from y=3.5 to y=1.2 over 80s (7:20-8:40)
                    {
                        float duration = 80f;
                        float pct = Mathf.Clamp01(phaseElapsed / duration);
                        float startY = 3.5f;
                        float endY = 1.2f;
                        t.localPosition = new Vector3(0.0f, Mathf.Lerp(startY, endY, pct), 14.0f);
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
