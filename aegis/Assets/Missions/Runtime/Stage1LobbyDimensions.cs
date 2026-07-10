namespace PinkSoft.Aegis.Missions
{
    /// <summary>Stage1 로비 BuildingKit 기준 치수 (Unity unit ≈ 1m).</summary>
    public static class Stage1LobbyDimensions
    {
        /// <summary>바닥(y=0)에서 천장 하면까지 높이.</summary>
        public const float CeilingHeight = 18f;

        public const float WallHeight = CeilingHeight;
        public static float WallCenterY => WallHeight * 0.5f;

        /// <summary>대리석 기둥 mesh scale Y.</summary>
        public const float ColumnScaleY = 9f;

        public static float ColumnCenterY => WallCenterY;

        /// <summary>후면 NEXA CORE 사인 패널 중심 높이.</summary>
        public const float BackSignCenterY = 12f;

        /// <summary>2층 발코니 컷 카메라 look-at / 스폰 고도.</summary>
        public const float BalconyHeight = 11f;

        /// <summary>2층 메자닌 바닥면 높이.</summary>
        public const float MezzanineFloorY = 10f;

        /// <summary>발코니 난간 상단 높이.</summary>
        public static float MezzanineRailingTopY => BalconyHeight + 0.35f;

        public const float MezzanineDeckThickness = 0.22f;

        /// <summary>2층 복도·사무실 천장 높이 (메자닌 바닥 + 4m).</summary>
        public const float MezzanineCeilingY = 14f;

        public const float MezzanineCeilingThickness = 0.2f;

        /// <summary>로비 외곽 반폭 (60m 정사각).</summary>
        public const float LobbyHalfSpan = 30f;

        public const float FrontWallZ = -30.2f;

        /// <summary>후면 주차장 연결 개구부 폭·높이.</summary>
        public const float ParkingPortalWidth = 16f;

        public const float ParkingPortalHeight = 6.5f;

        /// <summary>정면 회전문 개구부 폭.</summary>
        public const float EntrancePortalWidth = 14f;

        /// <summary>로비 후면(엘리베이터) Z.</summary>
        public const float BackWallZ = 30f;

        /// <summary>주차장 연장 구역 시작 Z.</summary>
        public const float ParkingStartZ = 30.5f;

        public const float ElevatorDoorHeight = 5.5f;

        /// <summary>공중 드론 스폰 고도.</summary>
        public const float DroneAirHeight = 13.5f;

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
