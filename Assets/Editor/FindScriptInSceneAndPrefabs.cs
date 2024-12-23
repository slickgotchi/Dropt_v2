using UnityEditor;
using UnityEngine;

public class FindScriptInSceneAndPrefabs : EditorWindow
{
    [MenuItem("Tools/Find ContentSizeFitterWithMax")]
    static void FindScript()
    {
        // Search in the active scene
        Debug.Log("Searching in the active scene...");
        var sceneObjectsWithScript = GameObject.FindObjectsOfType<UnityEngine.UI.ContentSizeFitterWithMax>();
        foreach (var obj in sceneObjectsWithScript)
        {
            Debug.Log($"Found ContentSizeFitterWithMax in Scene on GameObject: {obj.name}", obj.gameObject);
        }

        if (sceneObjectsWithScript.Length == 0)
        {
            Debug.Log("No GameObjects with ContentSizeFitterWithMax found in the scene.");
        }

        // Search in all prefabs
        Debug.Log("Searching in prefabs...");
        string[] guids = AssetDatabase.FindAssets("t:Prefab"); // Find all prefabs
        int foundCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null && prefab.GetComponentInChildren<UnityEngine.UI.ContentSizeFitterWithMax>(true) != null)
            {
                Debug.Log($"Found ContentSizeFitterWithMax in Prefab: {prefab.name} at path: {path}", prefab);
                foundCount++;
            }
        }

        if (foundCount == 0)
        {
            Debug.Log("No prefabs with ContentSizeFitterWithMax found in the project.");
        }
    }
}
