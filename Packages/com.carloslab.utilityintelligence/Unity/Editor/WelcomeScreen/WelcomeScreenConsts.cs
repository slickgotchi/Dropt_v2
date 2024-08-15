namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class WelcomeScreenConsts
    {
        public const string MenuPath = FrameworkEditorConsts.BaseMenuPath + "/Welcome Screen";
        public const string VisualAssetPath = FrameworkEditorConsts.UIBuilderPath + "WelcomeScreen.uxml";
        public const string LightThemePath = FrameworkEditorConsts.UIBuilderPath + "WelcomeScreen_Light.uss";
        public const string DarkThemePath = FrameworkEditorConsts.UIBuilderPath + "WelcomeScreen_Dark.uss";
        
        public static readonly string WelcomeLabelText = $"Welcome to {FrameworkEditorConsts.FrameworkName}";
        public const string ThanksLabelText = "Thank you for choosing us! 🥰";

        public static readonly string FrameworkVersionLabelText = $"Framework Version: {FrameworkRuntimeConsts.FrameworkVersion}";

        public static readonly string SuportLabelText = @$"If you <b>like</b> this plugin, please consider <b>supporting us</b> by leaving a <b>5-star review</b> on <a href={FrameworkEditorConsts.AssetStoreUrl}>the Asset Store</a>. Your <b>positive feedback</b> allows us to <b>dedicate more time</b> to developing this plugin. Thank you so much! 🥰";
    }
}