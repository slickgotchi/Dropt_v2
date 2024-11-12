using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Netcode;

public class NetworkObjectDuplicatePrefabFinder : EditorWindow
{
    [MenuItem("Tools/Slick/Find Duplicate Prefabs with NetworkObject")]
    public static void FindDuplicateNetworkObjectPrefabs()
    {
        // Dictionary to keep track of prefab names and their paths
        Dictionary<string, List<string>> prefabDictionary = new Dictionary<string, List<string>>();

        // Get all prefab asset GUIDs in the project
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            // Get the path of the prefab
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // Check if the prefab has a NetworkObject component
            if (prefab != null && prefab.GetComponent<NetworkObject>() != null)
            {
                string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);

                // Check if the prefab name already exists in the dictionary
                if (prefabDictionary.ContainsKey(prefabName))
                {
                    // Add the path to the existing list of paths for this prefab name
                    prefabDictionary[prefabName].Add(path);
                }
                else
                {
                    // Create a new entry for this prefab name
                    prefabDictionary[prefabName] = new List<string> { path };
                }
            }
        }

        // Log duplicates
        foreach (var entry in prefabDictionary)
        {
            if (entry.Value.Count > 1)
            {
                Debug.Log($"Duplicate Prefab with NetworkObject Found: {entry.Key}");
                foreach (string path in entry.Value)
                {
                    Debug.Log($" - {path}");
                }
            }
        }

        Debug.Log("Duplicate prefab with NetworkObject search completed.");
    }
}
