using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class TMP_FontAssetSetter : EditorWindow
{
    // Font asset to be set
    public TMP_FontAsset fontAsset;

    // Checkbox for enabling word wrapping
    public bool enableWrapping;

    // Add the script to the Tools menu
    [MenuItem("Tools/Slick/Set TextMeshPro Font Asset")]
    public static void ShowWindow()
    {
        // Create the editor window
        GetWindow<TMP_FontAssetSetter>("Set TMP Font Asset");
    }

    // Display the window interface
    void OnGUI()
    {
        GUILayout.Label("Set Font Asset for all TextMeshPro Components", EditorStyles.boldLabel);

        // Drag and drop TMP_FontAsset
        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), false);

        if (fontAsset == null)
        {
            EditorGUILayout.HelpBox("Please assign a TMP_FontAsset to set.", MessageType.Warning);
        }

        // Checkbox to enable/disable word wrapping
        enableWrapping = EditorGUILayout.Toggle("Enable Wrapping for all TMP objects", enableWrapping);

        // Button to execute the search and replace
        if (GUILayout.Button("Set Font Asset") && fontAsset != null)
        {
            SetFontAssetForAllTextMeshProComponents();
            SetFontAssetForAllSceneTextMeshProComponents();
        }
    }

    // Method to set the font asset for all TextMeshPro components in the project assets
    void SetFontAssetForAllTextMeshProComponents()
    {
        // Find all TMP_Text components in the project assets
        string[] allAssetGuids = AssetDatabase.FindAssets("t:GameObject");

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (gameObject != null)
            {
                TMP_Text[] textComponents = gameObject.GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text tmp in textComponents)
                {
                    Undo.RecordObject(tmp, "Set TMP Font Asset"); // Allow for undo functionality
                    tmp.font = fontAsset;
                    if (enableWrapping)
                    {
                        tmp.enableWordWrapping = true;
                    }
                    EditorUtility.SetDirty(tmp); // Mark the component as dirty so the change is saved
                }
            }
        }

        // Save the changes made to the assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Font asset has been set for all TextMeshPro components in the project.");
    }

    // Method to set the font asset for all TextMeshPro components in all active scenes
    void SetFontAssetForAllSceneTextMeshProComponents()
    {
        // Iterate through all open scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.isLoaded)
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();

                foreach (GameObject rootObject in rootObjects)
                {
                    TMP_Text[] textComponents = rootObject.GetComponentsInChildren<TMP_Text>(true);

                    foreach (TMP_Text tmp in textComponents)
                    {
                        Undo.RecordObject(tmp, "Set TMP Font Asset");
                        tmp.font = fontAsset;
                        if (enableWrapping)
                        {
                            tmp.enableWordWrapping = true;
                        }
                        EditorUtility.SetDirty(tmp);
                    }
                }

                // Mark the scene as dirty to indicate unsaved changes
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        Debug.Log("Font asset and wrapping options have been set for all TextMeshPro components in the active scenes.");
    }
}
