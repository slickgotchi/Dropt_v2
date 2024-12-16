using UnityEditor;
using UnityEngine;
using Unity.Netcode;

public class NestedNetworkObjectChecker : EditorWindow
{
    private GameObject prefab;

    [MenuItem("Tools/Slick/Nested NetworkObject Checker")]
    public static void ShowWindow()
    {
        GetWindow<NestedNetworkObjectChecker>("NetworkObject Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("NetworkObject Checker", EditorStyles.boldLabel);

        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

        if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            EditorGUILayout.HelpBox("Please drag a prefab asset into the field above.", MessageType.Info);
        }
        else if (GUILayout.Button("Check for Nested NetworkObjects"))
        {
            CheckForNestedNetworkObjects();
        }
    }

    private void CheckForNestedNetworkObjects()
    {
        if (prefab == null)
        {
            Debug.LogError("No prefab selected.");
            return;
        }

        // Load the prefab's root GameObject
        GameObject root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));

        if (root == null)
        {
            Debug.LogError("Failed to load prefab contents.");
            return;
        }

        // Find all NetworkObject components in the prefab
        NetworkObject[] networkObjects = root.GetComponentsInChildren<NetworkObject>(true);

        // Check for nested NetworkObjects
        bool hasNested = false;
        foreach (var networkObject in networkObjects)
        {
            if (networkObject.gameObject != root)
            {
                Debug.LogWarning($"Nested NetworkObject found: {networkObject.gameObject.name}", networkObject.gameObject);
                hasNested = true;
            }
        }

        if (!hasNested)
        {
            Debug.Log("No nested NetworkObjects found in the prefab.");
        }

        // Cleanup loaded prefab contents
        PrefabUtility.UnloadPrefabContents(root);
    }
}
