namespace PinkSoft.Aegis.Missions
{
    /// <summary>Addressables 주소 및 에디터 플레이용 씬 경로.</summary>
    public static class StageSceneAddresses
    {
        public static readonly string[] All =
        {
            "stage/1_lobby",
            "stage/2_lab",
            "stage/3_datacenter",
            "stage/4_core"
        };

        public static readonly string[] EditorPaths =
        {
            "Assets/Scenes/Stages/Stage1_Lobby.unity",
            "Assets/Scenes/Stages/Stage2_Lab.unity",
            "Assets/Scenes/Stages/Stage3_Datacenter.unity",
            "Assets/Scenes/Stages/Stage4_Core.unity"
        };

        public static readonly string[] Stage1Only = { "stage/1_lobby" };

        public static readonly string[] Stage1OnlyEditorPaths =
        {
            "Assets/Scenes/Stages/Stage1_Lobby.unity"
        };
    }
}
