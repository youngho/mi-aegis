#nullable enable
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

namespace PinkSoft.Aegis.Missions
{
    public sealed class Stage3CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AegisMissionController? missionController;
        [SerializeField] private CinemachineCamera[] virtualCameras = new CinemachineCamera[7];

        [Header("Speed Settings (for testing)")]
        [SerializeField] private float timeScale = 1f;

        private int _currentVcamIndex = -1;
        private float _customElapsed;
        private bool _useCustomTimeline;

        // cut_timeline.md Stage3 타임코드 → 초
        private readonly float[] _startTimes =
        {
            0f,     // 3-1 Entrance
            80f,    // 3-2 Smoke
            170f,   // 3-3 Sub-server
            280f,   // 3-4 Power (transition 260s)
            370f,   // 3-5 Bomb
            460f,   // 3-6 Cooling
            570f    // 3-7 Boss (transition 550s)
        };

        private readonly string[] _cameraNames =
        {
            "Vcam_3_1_Entrance",
            "Vcam_3_2_Smoke",
            "Vcam_3_3_SubServer",
            "Vcam_3_4_PowerControl",
            "Vcam_3_5_BombDefusal",
            "Vcam_3_6_Cooling",
            "Vcam_3_7_Boss"
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
                    t.localPosition = new Vector3(0f, 1.6f, -20f);
                    t.localEulerAngles = new Vector3(5f, 0f, 0f);
                    break;
                case 1:
                    t.localPosition = new Vector3(-2f, 1.4f, -6f);
                    t.localEulerAngles = new Vector3(3f, 12f, 0f);
                    break;
                case 2:
                    t.localPosition = new Vector3(0f, 2f, 6f);
                    t.localEulerAngles = new Vector3(10f, 0f, 0f);
                    break;
                case 3:
                    t.localPosition = new Vector3(0f, 2.4f, 20f);
                    t.localEulerAngles = new Vector3(14f, 0f, 0f);
                    break;
                case 4:
                    t.localPosition = new Vector3(0f, 1.5f, 32f);
                    t.localEulerAngles = new Vector3(4f, 0f, 0f);
                    break;
                case 5:
                    t.localPosition = new Vector3(3f, 1.3f, 44f);
                    t.localEulerAngles = new Vector3(-2f, -18f, 0f);
                    break;
                case 6:
                    t.localPosition = new Vector3(0f, 1.8f, 52f);
                    t.localEulerAngles = new Vector3(18f, 0f, 0f);
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
            Debug.Log($"[Stage3CameraController] Switched to {_cameraNames[index]} at {GetElapsedTime():F2}s");
        }

        private void AnimateCameraMovement(int index, float elapsed)
        {
            if (index < 0 || index >= virtualCameras.Length || virtualCameras[index] == null) return;

            var t = virtualCameras[index].transform;
            float phaseStart = _startTimes[index];
            float phaseElapsed = elapsed - phaseStart;
            float phaseDuration = index + 1 < _startTimes.Length ? _startTimes[index + 1] - phaseStart : 210f;
            float pct = EaseInOut(Mathf.Clamp01(phaseElapsed / phaseDuration));

            switch (index)
            {
                case 0: // 서버실 입구 — 스파크 연출, 와이드 고정
                    {
                        float creep = Mathf.Clamp01(phaseElapsed / 18f);
                        t.localPosition = Vector3.Lerp(
                            new Vector3(0f, 1.6f, -20f),
                            new Vector3(0f, 1.55f, -14f),
                            creep);
                        var sparkFlicker = Mathf.Sin(phaseElapsed * 8f) * 0.12f;
                        t.localEulerAngles = new Vector3(5f + sparkFlicker, 0f, sparkFlicker * 0.3f);
                    }
                    break;

                case 1: // 연기 자욱 통로 — 시야 제한, 천천히 전진
                    t.localPosition = Vector3.Lerp(
                        new Vector3(-2f, 1.4f, -6f),
                        new Vector3(1.5f, 1.35f, 2f),
                        pct);
                    t.localEulerAngles = new Vector3(3f, Mathf.Lerp(12f, -8f, pct), 0f);
                    break;

                case 2: // 서브서버 파괴 — 랙 사이 팬
                    {
                        float pan = Mathf.Lerp(-20f, 20f, pct);
                        t.localPosition = new Vector3(Mathf.Lerp(-2f, 2f, pct), 2f, Mathf.Lerp(6f, 14f, pct));
                        t.localEulerAngles = new Vector3(10f, pan, 0f);
                    }
                    break;

                case 3: // 전력 제어소 — 상하 2층 시점
                    t.localPosition = Vector3.Lerp(
                        new Vector3(0f, 2.4f, 20f),
                        new Vector3(0f, 1.2f, 28f),
                        pct);
                    t.localEulerAngles = new Vector3(Mathf.Lerp(14f, 2f, pct), 0f, 0f);
                    break;

                case 4: // 폭탄 해체 — 콘솔 고정
                    t.localPosition = new Vector3(0f, 1.5f, 32f);
                    t.localEulerAngles = new Vector3(4f + Mathf.Sin(phaseElapsed * 2f) * 0.5f, 0f, 0f);
                    break;

                case 5: // 냉각 구역 — 증기 속 스윕
                    t.localPosition = Vector3.Lerp(
                        new Vector3(3f, 1.3f, 44f),
                        new Vector3(-3f, 1.25f, 50f),
                        pct);
                    t.localEulerAngles = new Vector3(-2f, Mathf.Lerp(-18f, 16f, pct), 0f);
                    break;

                case 6: // 오버로드 보스 — 360° 회전
                    {
                        float orbit = pct * 360f;
                        t.localPosition = new Vector3(
                            Mathf.Sin(orbit * Mathf.Deg2Rad) * 3f,
                            1.8f + Mathf.Sin(pct * Mathf.PI) * 0.5f,
                            Mathf.Lerp(52f, 58f, pct));
                        t.localEulerAngles = new Vector3(18f, orbit, 0f);
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
