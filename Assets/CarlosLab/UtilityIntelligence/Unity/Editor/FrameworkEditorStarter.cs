#region

using UnityEditor;

#endregion

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class FrameworkEditorStarter
    {
        [InitializeOnLoadMethod]
        private static void StartUp()
        {
            FrameworkEditorPrefs.LoadData();
            WelcomeScreenWindow.StartUp();
        }
    }
}