using System;
using UnityEngine;

namespace PinkSoft.Aegis.Missions
{
    /// <summary>Stage 1~4 순차 활성화. 각 스테이지 루트는 완료 시 OnStageComplete를 호출합니다.</summary>
    public sealed class StageManager : MonoBehaviour
    {
        [SerializeField] GameObject[] stages = Array.Empty<GameObject>();

        int _currentIndex = -1;

        public event Action<int>? OnStageStarted;
        public event Action<int>? OnAllStagesComplete;

        public int CurrentStageIndex => _currentIndex;
        public int StageCount => stages.Length;

        public void Begin()
        {
            for (var i = 0; i < stages.Length; i++)
            {
                if (stages[i] != null)
                    stages[i].SetActive(false);
            }

            Advance();
        }

        public void Advance()
        {
            if (_currentIndex >= 0 && _currentIndex < stages.Length && stages[_currentIndex] != null)
                stages[_currentIndex].SetActive(false);

            _currentIndex++;
            if (_currentIndex >= stages.Length)
            {
                OnAllStagesComplete?.Invoke();
                return;
            }

            if (stages[_currentIndex] != null)
                stages[_currentIndex].SetActive(true);

            OnStageStarted?.Invoke(_currentIndex);
        }

        public void NotifyStageComplete()
        {
            ReportEvent(ScoreEventType.TimeBonus, $"stage{_currentIndex + 1}_clear");
            Advance();
        }

        public event Action<PinkSoft.MissionSDK.ScoreEventType, string>? ReportEventRequested;

        void ReportEvent(PinkSoft.MissionSDK.ScoreEventType eventType, string targetId)
        {
            ReportEventRequested?.Invoke(eventType, targetId);
        }
    }
}
