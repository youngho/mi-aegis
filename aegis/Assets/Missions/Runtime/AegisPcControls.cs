namespace PinkSoft.Aegis.Missions
{
    /// <summary>PC(마우스·키보드) 공통 단축키 정의. README §3.5 와 동기화.</summary>
    public static class AegisPcControls
    {
        public const string BriefingSceneName = "Mission Briefing";

        public static class Actions
        {
            public const string Fire = "사격";
            public const string AcceptMission = "미션 수락";
            public const string SkipBriefingText = "브리핑 텍스트 스킵";
            public const string RejectBriefing = "브리핑 재시작";
            public const string ToggleHelp = "단축키 도움말";
            public const string ReloadScene = "씬 다시 시작";
            public const string ReturnToBriefing = "브리핑으로";
            public const string SkipStage = "스테이지 스킵(디버그)";
            public const string JumpCut = "컷 점프 (1~7)";
            public const string PrevCut = "이전 컷";
            public const string NextCut = "다음 컷";
            public const string ToggleTimeline = "카메라 타임라인 토글";
            public const string PathPreviewPlay = "경로 미리보기 재생/정지";
            public const string PathPreviewReset = "경로 미리보기 처음으로";
        }

        public static class Keys
        {
            public const string Fire = "마우스 좌클릭 / Space";
            public const string AcceptMission = "Enter / Space / ACCEPT 버튼 클릭";
            public const string SkipBriefingText = "Space (타이핑 중)";
            public const string RejectBriefing = "Esc";
            public const string ToggleHelp = "F1 / H";
            public const string ReloadScene = "R";
            public const string ReturnToBriefing = "Esc (미션·스테이지)";
            public const string SkipStage = "N";
            public const string JumpCut = "1 ~ 7";
            public const string PrevCut = "[";
            public const string NextCut = "]";
            public const string ToggleTimeline = "T";
            public const string PathPreviewPlay = "P";
            public const string PathPreviewReset = "0";
        }
    }
}
