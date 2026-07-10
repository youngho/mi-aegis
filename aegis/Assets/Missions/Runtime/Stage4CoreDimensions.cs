namespace PinkSoft.Aegis.Missions
{
    /// <summary>Stage4 최상층 펜트하우스 & 코어 치수 (Unity unit ≈ 1m).</summary>
    public static class Stage4CoreDimensions
    {
        public const float CeilingHeight = 4.5f;
        public static float WallCenterY => CeilingHeight * 0.5f;

        /// <summary>일반 복도 폭.</summary>
        public const float CorridorWidth = 8f;

        /// <summary>회장실 및 보스 방 폭.</summary>
        public const float PenthouseWidth = 16f;

        /// <summary>드론/공중 스폰 고도.</summary>
        public const float AirSpawnHeight = 3.4f;

        /// <summary>저격/천장 스폰.</summary>
        public const float CeilingSpawnHeight = 3.8f;

        /// <summary>보스 부양 높이.</summary>
        public const float BossHoverHeight = 4.5f;

        // 컷 앵커 Z (카메라 레일 기준)
        public const float Cut4_1_EntranceZ = -20f;
        public const float Cut4_2_OfficeZ = -6f;
        public const float Cut4_3_BalconyZ = 8f;
        public const float Cut4_4_CorridorZ = 22f;
        public const float Cut4_5_MainframeAccessZ = 36f;
        public const float Cut4_6_BossP1Z = 50f;
        public const float Cut4_7_BossP2Z = 66f;
    }
}
