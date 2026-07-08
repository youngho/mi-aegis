#nullable enable
using System;
using System.Collections.Generic;
using PinkSoft.MissionSDK;
using UnityEngine;

namespace PinkSoft.Aegis.Missions
{
    /// <summary>AEGIS 레일 슈터 — mi Core IMissionController 구현.</summary>
    public sealed class AegisMissionController : MonoBehaviour, IMissionController
    {
        [SerializeField] LayerMask targetLayer;
        [SerializeField] StageManager stageManager = null!;

        readonly MissionInputSubscription _inputSub = new();
        readonly List<ScoreEventRecord> _log = new();

        float _elapsed;
        float _timeLimit = 3600f;
        int _comboCount;
        int _displayScore;
        bool _ended;

        public event Action<int>? OnScoreChanged;
        public event Action<bool, MissionResultData>? OnMissionEnded;
        public event Action<MissionError>? OnError;

        void Awake()
        {
            if (stageManager == null)
                stageManager = GetComponentInChildren<StageManager>(true);
        }

        public void InitializeMission(RuntimeUserData userData, MissionContext context)
        {
            _elapsed = 0f;
            _comboCount = 0;
            _displayScore = 0;
            _ended = false;
            _log.Clear();

            if (context.Config.timeLimitSeconds > 0)
                _timeLimit = context.Config.timeLimitSeconds;

            if (context.Input == null)
            {
                OnError?.Invoke(new MissionError
                {
                    code = MissionErrorCode.InitializationFailed,
                    message = "MissionContext.Input is null"
                });
                return;
            }

            if (stageManager == null)
            {
                OnError?.Invoke(new MissionError
                {
                    code = MissionErrorCode.InitializationFailed,
                    message = "StageManager is not assigned"
                });
                return;
            }

            stageManager.ReportEventRequested -= OnStageReportEvent;
            stageManager.ReportEventRequested += OnStageReportEvent;
            stageManager.OnAllStagesComplete -= OnAllStagesComplete;
            stageManager.OnAllStagesComplete += OnAllStagesComplete;

            _inputSub.Subscribe(context.Input, HandleHit);
            stageManager.Begin();
        }

        void Update()
        {
            if (_ended) return;
            _elapsed += Time.deltaTime;
            if (_elapsed >= _timeLimit)
                Finish(false);
        }

        void HandleHit(InputHit hit)
        {
            if (_ended) return;

            if (!MissionHitUtility.TryRaycast(hit, targetLayer, out var rh))
            {
                _comboCount = 0;
                return;
            }

            var targetId = rh.collider.name;
            if (targetId.StartsWith("hostage", StringComparison.OrdinalIgnoreCase))
            {
                _comboCount = 0;
                RegisterEvent(ScoreEventType.Penalty, targetId);
                return;
            }

            if (targetId.StartsWith("boss_", StringComparison.OrdinalIgnoreCase))
            {
                RegisterEvent(ScoreEventType.ObjectiveComplete, targetId);
                return;
            }

            _comboCount++;
            RegisterEvent(ScoreEventType.TargetHit, targetId);
            if (_comboCount >= 3)
                RegisterEvent(ScoreEventType.Combo, $"combo_{_comboCount}");
        }

        void OnStageReportEvent(ScoreEventType eventType, string targetId)
        {
            RegisterEvent(eventType, targetId);
        }

        void OnAllStagesComplete()
        {
            Finish(true);
        }

        void RegisterEvent(ScoreEventType eventType, string targetId)
        {
            ReportEvent(eventType, targetId);

            var delta = eventType switch
            {
                ScoreEventType.TargetHit => 100,
                ScoreEventType.Combo => 50,
                ScoreEventType.TimeBonus => 200,
                ScoreEventType.ObjectiveComplete => 500,
                ScoreEventType.Penalty => -150,
                _ => 0
            };

            _displayScore = Mathf.Max(0, _displayScore + delta);
            OnScoreChanged?.Invoke(_displayScore);
        }

        public void ReportEvent(ScoreEventType eventType, string targetId)
        {
            _log.Add(new ScoreEventRecord
            {
                eventType = eventType,
                targetId = targetId,
                timestampMs = (int)(_elapsed * 1000f)
            });
        }

        void Finish(bool success)
        {
            if (_ended) return;
            _ended = true;
            _inputSub.Unsubscribe();

            if (stageManager != null)
            {
                stageManager.ReportEventRequested -= OnStageReportEvent;
                stageManager.OnAllStagesComplete -= OnAllStagesComplete;
            }

            OnMissionEnded?.Invoke(success, new MissionResultData
            {
                finalScore = _displayScore,
                playTime = (int)_elapsed,
                eventLog = new List<ScoreEventRecord>(_log)
            });
        }

        public void OnPause() => enabled = false;
        public void OnResume() => enabled = true;

        public void Shutdown()
        {
            _inputSub.Unsubscribe();
            _ended = true;

            if (stageManager != null)
            {
                stageManager.ReportEventRequested -= OnStageReportEvent;
                stageManager.OnAllStagesComplete -= OnAllStagesComplete;
            }
        }
    }
}
