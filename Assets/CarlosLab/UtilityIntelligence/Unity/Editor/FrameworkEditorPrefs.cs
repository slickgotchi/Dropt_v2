#region

using Newtonsoft.Json;
using UnityEditor;

#endregion

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class FrameworkEditorPrefs
    {
        private static FrameworkPrefsData data;

        private static FrameworkPrefsData Data
        {
            get
            {
                if (data == null)
                    data = new FrameworkPrefsData();

                return data;
            }
        }

        public static bool HideWelcomeScreen
        {
            get => Data.HideWelcomeScreen;
            set
            {
                if (Data.HideWelcomeScreen == value)
                    return;

                Data.HideWelcomeScreen = value;
                Save();
            }
        }

        public static string FrameworkVersion
        {
            get => Data.FrameworkVersion;
            set
            {
                if (Data.FrameworkVersion == value)
                    return;

                Data.FrameworkVersion = value;
                Save();
            }
        }

        public static void LoadData()
        {
            string prefs = EditorPrefs.GetString(FrameworkEditorConsts.EditorPrefsKey);
            if (!string.IsNullOrEmpty(prefs))
                data = JsonConvert.DeserializeObject<FrameworkPrefsData>(prefs);
        }

        private static void Save()
        {
            EditorPrefs.SetString(FrameworkEditorConsts.EditorPrefsKey, JsonConvert.SerializeObject(Data));
        }
    }
}