using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EnsureBootstrapScene
{
    private const string BootstrapSceneName = "Bootstrap";
    private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity"; // Adjust the path as needed

    private static string previousScenePath;

    static EnsureBootstrapScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Store the current scene path before switching to Bootstrap
            previousScenePath = SceneManager.GetActiveScene().path;

            Debug.Log("save scene path: " + previousScenePath);

            // Save the current scene
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Set the Bootstrap scene to be the first scene in play mode
            EditorSceneManager.OpenScene(BootstrapScenePath);
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EnsureBootstrapIsFirstScene();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Return to the previous scene after play mode
            if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != BootstrapScenePath)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorSceneManager.OpenScene(previousScenePath);
                };
            }
        }
    }

    private static void EnsureBootstrapIsFirstScene()
    {
        // Check if the Bootstrap scene is in the build settings
        var buildScenes = EditorBuildSettings.scenes;
        bool bootstrapSceneExists = false;
        foreach (var scene in buildScenes)
        {
            if (scene.path == BootstrapScenePath)
            {
                bootstrapSceneExists = true;
                break;
            }
        }

        if (!bootstrapSceneExists)
        {
            // Add the Bootstrap scene to the build settings
            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(BootstrapScenePath, true);
            for (int i = 0; i < buildScenes.Length; i++)
            {
                newScenes[i + 1] = buildScenes[i];
            }
            EditorBuildSettings.scenes = newScenes;
        }
        else
        {
            // Ensure Bootstrap scene is the first in the build settings
            if (buildScenes[0].path != BootstrapScenePath)
            {
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    if (buildScenes[i].path == BootstrapScenePath)
                    {
                        // Swap the Bootstrap scene to the first position
                        var temp = buildScenes[0];
                        buildScenes[0] = buildScenes[i];
                        buildScenes[i] = temp;
                        EditorBuildSettings.scenes = buildScenes;
                        break;
                    }
                }
            }
        }
    }
}
