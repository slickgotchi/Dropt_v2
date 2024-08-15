namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class FrameworkEditorConsts
    {
        public const string FrameworkId = "UtilityIntelligence";
        public const string FrameworkName = "Utility Intelligence";
        public const string DocumentationUrl = "https://carloslab-ai.github.io/UtilityIntelligence/";
        public const string DiscordUrl = "https://discord.gg/vRFEK5uE3f";
        public const string AssetStoreUrl = "https://assetstore.unity.com/packages/slug/276632";
        public const string EditorPrefsKey = "CarlosLab." + FrameworkId + ".EditorPrefs";
        public const string BaseMenuPath = "Tools/CarlosLab/" + FrameworkName;
        public const string PackagePath = "Packages/com.carloslab.utilityintelligence/";
        public const string UIBuilderPath = PackagePath + "Unity/Editor/UIBuilder/";

        
        public const string CreateUtilityWorldMenuPath = "GameObject/CarlosLab/Utility World";
        public const string UtilityWorldPrefabPath = PackagePath + "Unity/Runtime/UtilityWorld.prefab";
        
        public const string CreateRuntimeEditorMenuPath = "GameObject/CarlosLab/Utility Intelligence Runtime Editor";
        public const string RuntimeEditorPrefabPath = PackagePath + "Unity/UI/Views/UtilityIntelligenceView/Runtime/UtilityIntelligenceRuntimeEditor.prefab";
    }
}