namespace PinkSoft.Aegis.Missions
{
    /// <summary>Stage3 데이터센터 치수 (Unity unit ≈ 1m).</summary>
    public static class Stage3DatacenterDimensions
    {
        public const float CeilingHeight = 4.5f;
        public static float WallCenterY => CeilingHeight * 0.5f;

        /// <summary>서버랙 미로 복도 폭.</summary>
        public const float MazeCorridorWidth = 8f;

        /// <summary>연기/냉각 구역 폭.</summary>
        public const float SteamZoneWidth = 10f;

        /// <summary>제어소·보스 아레나 폭.</summary>
        public const float ControlHallWidth = 16f;

        /// <summary>서버랙 중심 높이.</summary>
        public const float ServerRackCenterY = 1.1f;

        /// <summary>드론/공중 스폰 고도.</summary>
        public const float AirSpawnHeight = 3.4f;

        /// <summary>저격/천장 스폰.</summary>
        public const float CeilingSpawnHeight = 3.8f;

        /// <summary>보스 드론 부양 높이.</summary>
        public const float BossHoverHeight = 4.5f;

        // 컷 앵커 Z (카메라 레일 기준)
        public const float Cut3_1_EntranceZ = -14f;
        public const float Cut3_2_SmokeZ = -2f;
        public const float Cut3_3_SubServerZ = 10f;
        public const float Cut3_4_PowerControlZ = 24f;
        public const float Cut3_5_BombDefusalZ = 36f;
        public const float Cut3_6_CoolingZ = 48f;
        public const float Cut3_7_BossZ = 60f;
    }
}
