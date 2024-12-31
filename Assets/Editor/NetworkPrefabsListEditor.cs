using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkPrefabsListEditor : EditorWindow
{
    private NetworkPrefabsList networkPrefabsList;

    [MenuItem("Tools/Slick/Network Prefabs List Updater")]
    public static void ShowWindow()
    {
        GetWindow<NetworkPrefabsListEditor>("Network Prefabs List Updater");
    }

    private void OnEnable()
    {
        // Try to find and assign the DefaultNetworkPrefabs asset in the project
        AssignDefaultNetworkPrefabs();
    }

    private void OnGUI()
    {
        GUILayout.Label("Network Prefabs List Updater", EditorStyles.boldLabel);

        networkPrefabsList = (NetworkPrefabsList)EditorGUILayout.ObjectField(
            "Network Prefabs List",
            networkPrefabsList,
            typeof(NetworkPrefabsList),
            false
        );

        if (networkPrefabsList == null)
        {
            EditorGUILayout.HelpBox("Please assign a Network Prefabs List.", MessageType.Warning);
        }
        else
        {
            if (GUILayout.Button("Update Network Prefabs List"))
            {
                UpdateNetworkPrefabsList();
            }
        }
    }

    private void UpdateNetworkPrefabsList()
    {
        // Find all NetworkObject prefabs in the project
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        List<NetworkPrefab> newNetworkPrefabs = new List<NetworkPrefab>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponent<NetworkObject>() != null)
            {
                NetworkPrefab networkPrefab = new NetworkPrefab { Prefab = prefab };
                newNetworkPrefabs.Add(networkPrefab);
            }
        }

        // Track changes for logging
        var existingPrefabs = new List<NetworkPrefab>(networkPrefabsList.PrefabList);
        var removedPrefabs = new List<NetworkPrefab>(existingPrefabs);
        var addedPrefabs = new List<NetworkPrefab>();

        // Remove all existing prefabs from the list
        foreach (var prefab in existingPrefabs)
        {
            networkPrefabsList.Remove(prefab);
        }

        // Add new prefabs to the list
        foreach (var prefab in newNetworkPrefabs)
        {
            if (!existingPrefabs.Contains(prefab))
            {
                addedPrefabs.Add(prefab);
            }
            removedPrefabs.Remove(prefab);
            networkPrefabsList.Add(prefab);
        }

        // Save changes
        EditorUtility.SetDirty(networkPrefabsList);
        AssetDatabase.SaveAssets();

        // Log details of the operation
        Debug.Log("Network Prefabs List updated successfully.");

        if (addedPrefabs.Count > 0)
        {
            Debug.Log($"Added {addedPrefabs.Count} prefab(s):");
            foreach (var prefab in addedPrefabs)
            {
                Debug.Log($"    Added: {prefab.Prefab.name}");
            }
        }
        else
        {
            Debug.Log("No new prefabs were added.");
        }

        if (removedPrefabs.Count > 0)
        {
            Debug.Log($"Removed {removedPrefabs.Count} prefab(s):");
            foreach (var prefab in removedPrefabs)
            {
                Debug.Log($"    Removed: {prefab.Prefab.name}");
            }
        }
        else
        {
            Debug.Log("No prefabs were removed.");
        }

        int unchangedCount = newNetworkPrefabs.Count - addedPrefabs.Count;
        Debug.Log($"{unchangedCount} prefab(s) remained unchanged.");
    }

    private void AssignDefaultNetworkPrefabs()
    {
        // Find the first asset of type NetworkPrefabsList in the project
        string[] guids = AssetDatabase.FindAssets("t:NetworkPrefabsList");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            networkPrefabsList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(path);
            Debug.Log($"Assigned Default NetworkPrefabsList: {path}");
        }
        else
        {
            Debug.LogWarning("No NetworkPrefabsList asset found in the project.");
        }
    }
}
