using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using System.IO;

public class EnableTextMeshProWordWrap : EditorWindow
{
    [MenuItem("Tools/Slick/Enable Word Wrapping for TextMeshPro")]
    public static void ShowWindow()
    {
        GetWindow<EnableTextMeshProWordWrap>("Enable Word Wrapping for TextMeshPro");
    }

    void OnGUI()
    {
        GUILayout.Label("Enable Word Wrapping for all TextMeshPro (UI) components in all scenes and prefabs", EditorStyles.boldLabel);

        if (GUILayout.Button("Find and Enable Word Wrapping in All Scenes and Prefabs"))
        {
            EnableWordWrappingInAllScenes();
            EnableWordWrappingInPrefabs();
        }
    }

    // Enable word wrapping for TextMeshPro components in all scenes in the project
    void EnableWordWrappingInAllScenes()
    {
        // Get all the scene paths in the project
        string[] allScenePaths = AssetDatabase.FindAssets("t:Scene");
        int sceneCount = 0;

        foreach (string guid in allScenePaths)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            if (sceneAsset != null)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Find all TextMeshProUGUI components in the scene
                TextMeshProUGUI[] textMeshProComponents = FindObjectsOfType<TextMeshProUGUI>();
                bool sceneModified = false;

                foreach (var textMeshPro in textMeshProComponents)
                {
                    if (!textMeshPro.enableWordWrapping)
                    {
                        Undo.RecordObject(textMeshPro, "Enable Word Wrapping"); // Allow undo
                        textMeshPro.enableWordWrapping = true;
                        EditorUtility.SetDirty(textMeshPro); // Mark object as dirty for saving
                        sceneModified = true;
                    }
                }

                if (sceneModified)
                {
                    // Save the modified scene
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                    sceneCount++;
                }
            }
        }

        Debug.Log($"Enabled Word Wrapping on TextMeshPro (UI) components in {sceneCount} scenes.");
    }

    // Enable word wrapping for TextMeshPro components in all prefabs in the project
    void EnableWordWrappingInPrefabs()
    {
        string[] allPrefabPaths = AssetDatabase.GetAllAssetPaths();
        int prefabCount = 0;

        foreach (string prefabPath in allPrefabPaths)
        {
            if (prefabPath.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    // Get all TextMeshProUGUI components in the prefab
                    TextMeshProUGUI[] textMeshProComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

                    bool prefabModified = false;

                    foreach (var textMeshPro in textMeshProComponents)
                    {
                        if (!textMeshPro.enableWordWrapping)
                        {
                            textMeshPro.enableWordWrapping = true;
                            EditorUtility.SetDirty(textMeshPro); // Mark as dirty for saving
                            prefabModified = true;
                        }
                    }

                    if (prefabModified)
                    {
                        // Apply changes to the prefab
                        PrefabUtility.SavePrefabAsset(prefab);
                        prefabCount++;
                    }
                }
            }
        }

        Debug.Log($"Enabled Word Wrapping on {prefabCount} TextMeshPro (UI) components in prefabs.");
    }
}
