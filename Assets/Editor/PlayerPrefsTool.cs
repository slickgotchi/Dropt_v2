using UnityEngine;
using UnityEditor;

public class PlayerPrefsTool : EditorWindow
{
    [MenuItem("Tools/PlayerPrefs/Delete All PlayerPrefs")]
    public static void DeleteAllPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog("Delete All PlayerPrefs", "Are you sure you want to delete all PlayerPrefs? This action cannot be undone.", "Yes", "No"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("All PlayerPrefs have been deleted.");
        }
    }

    [MenuItem("Tools/PlayerPrefs/Open PlayerPrefs Editor")]
    public static void OpenWindow()
    {
        GetWindow<PlayerPrefsTool>("PlayerPrefs Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("PlayerPrefs Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Delete All PlayerPrefs"))
        {
            DeleteAllPlayerPrefs();
        }
    }
}
