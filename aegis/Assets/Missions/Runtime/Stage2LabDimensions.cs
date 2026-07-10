namespace PinkSoft.Aegis.Missions
{
    /// <summary>Stage2 연구실 BuildingKit 기준 치수 (Unity unit ≈ 1m).</summary>
    public static class Stage2LabDimensions
    {
        public const float CeilingHeight = 4.2f;
        public static float WallCenterY => CeilingHeight * 0.5f;

        /// <summary>복도 폭.</summary>
        public const float CorridorWidth = 6f;

        /// <summary>실험실 홀 폭.</summary>
        public const float LabHallWidth = 14f;

        /// <summary>배양 탱크 중심 높이.</summary>
        public const float IncubationTankCenterY = 1.6f;

        /// <summary>서버랙 중심 높이.</summary>
        public const float ServerRackCenterY = 1.1f;

        /// <summary>드론/공중 스폰 고도.</summary>
        public const float AirSpawnHeight = 3.2f;

        /// <summary>2층/천장 기습 스폰.</summary>
        public const float CeilingSpawnHeight = 3.6f;

        // 컷 앵커 Z (카메라 레일 기준)
        public const float Cut2_1_CorridorZ = -14f;
        public const float Cut2_2_IncubationZ = -4f;
        public const float Cut2_3_LabZoneZ = 8f;
        public const float Cut2_4_IsolatedZ = 18f;
        public const float Cut2_5_DataStorageZ = 28f;
        public const float Cut2_6_ShadowZ = 38f;
        public const float Cut2_7_BossZ = 46f;
    }
}
