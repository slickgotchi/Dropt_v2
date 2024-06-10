using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Netcode;

[CustomEditor(typeof(Prefabs_NetworkObject))]
public class Prefabs_NetworkObject_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Prefabs_NetworkObject prefabsNetworkObject = (Prefabs_NetworkObject)target;
        if (GUILayout.Button("Populate Network Objects"))
        {
            List<GameObject> networkObjects = FindAllNetworkObjectsInProject();
            prefabsNetworkObject.PopulateNetworkObjects(networkObjects);
            EditorUtility.SetDirty(prefabsNetworkObject);
        }
    }

    private List<GameObject> FindAllNetworkObjectsInProject()
    {
        List<GameObject> networkObjectsInProject = new List<GameObject>();
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

        foreach (string assetPath in allAssetPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null && 
                prefab.GetComponent<NetworkObject>() != null && 
                prefab.GetComponent<NetworkLevel>() == null)
            {
                networkObjectsInProject.Add(prefab);
            }
        }

        return networkObjectsInProject;
    }
}
