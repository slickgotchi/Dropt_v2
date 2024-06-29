using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkPrefabsListEditor : EditorWindow
{
    private NetworkPrefabsList networkPrefabsList;

    [MenuItem("Tools/Network Prefabs List Updater")]
    public static void ShowWindow()
    {
        GetWindow<NetworkPrefabsListEditor>("Network Prefabs List Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("Network Prefabs List Updater", EditorStyles.boldLabel);

        networkPrefabsList = (NetworkPrefabsList)EditorGUILayout.ObjectField("Network Prefabs List", networkPrefabsList, typeof(NetworkPrefabsList), false);

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

        // Update the NetworkPrefabsList
        // Remove existing prefabs
        var existingPrefabs = new List<NetworkPrefab>(networkPrefabsList.PrefabList);
        foreach (var prefab in existingPrefabs)
        {
            networkPrefabsList.Remove(prefab);
        }

        // Add new prefabs
        foreach (var prefab in newNetworkPrefabs)
        {
            networkPrefabsList.Add(prefab);
        }

        EditorUtility.SetDirty(networkPrefabsList);
        AssetDatabase.SaveAssets();

        Debug.Log("Network Prefabs List updated successfully.");
    }
}
