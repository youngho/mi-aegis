namespace PinkSoft.Aegis.Missions
{
    /// <summary>Stage1 로비 BuildingKit 기준 치수 (Unity unit ≈ 1m).</summary>
    public static class Stage1LobbyDimensions
    {
        /// <summary>바닥(y=0)에서 천장 하면까지 높이.</summary>
        public const float CeilingHeight = 10f;

        public const float WallHeight = CeilingHeight;
        public static float WallCenterY => WallHeight * 0.5f;

        /// <summary>대리석 기둥 mesh scale Y.</summary>
        public const float ColumnScaleY = 5f;

        public static float ColumnCenterY => WallCenterY;

        /// <summary>후면 NEXA CORE 사인 패널 중심 높이.</summary>
        public const float BackSignCenterY = 7f;

        /// <summary>2층 발코니 컷 카메라 look-at / 스폰 고도.</summary>
        public const float BalconyHeight = 7.5f;

        /// <summary>공중 드론 스폰 고도.</summary>
        public const float DroneAirHeight = 6.5f;

        public const float LegacyCeilingHeight = 5f;

        public static float ScaleFromLegacy => CeilingHeight / LegacyCeilingHeight;

        public static float ExteriorCanopyY => 4.6f * ScaleFromLegacy;
        public static float ExteriorFrameCenterY => WallCenterY;
        public static float ExteriorFrameHeight => WallHeight;
        public static float ExteriorDoorCenterY => 1.2f * ScaleFromLegacy;
        public static float ExteriorDoorScaleY => 1.2f * ScaleFromLegacy;
        public static float GlassFacadeCenterY => 3.5f * ScaleFromLegacy;
        public static float GlassPanelHeight => 6.5f * ScaleFromLegacy;
        public static float ExteriorSignY => 5.2f * ScaleFromLegacy;
        public static float CityBackdropY => 4.5f * ScaleFromLegacy;
        public static float CityBackdropEntranceY => 3f * ScaleFromLegacy;
        public static float ExteriorRimLightY => 6f * ScaleFromLegacy;
    }
}
