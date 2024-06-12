using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReplaceMissingPrefabs : MonoBehaviour
{
    [MenuItem("Tools/Replace Missing Prefabs")]
    static void ReplaceMissingPrefabsInSelection()
    {
        // Get selected GameObjects in the editor
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected. Please select one or more GameObjects in the hierarchy.");
            return;
        }

        // List to collect all objects to be processed
        List<GameObject> objectsToProcess = new List<GameObject>();

        foreach (GameObject obj in selectedObjects)
        {
            CollectObjectsWithMissingPrefabs(obj, objectsToProcess);
        }

        // Process collected objects
        foreach (GameObject obj in objectsToProcess)
        {
            ReplaceMissingPrefab(obj);
        }

        Debug.Log("Missing prefabs replaced with simple GameObjects.");
    }

    static void CollectObjectsWithMissingPrefabs(GameObject obj, List<GameObject> objectsToProcess)
    {
        // Check if the GameObject is a prefab instance and has a missing prefab
        PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(obj);

        if (prefabAssetType == PrefabAssetType.MissingAsset)
        {
            objectsToProcess.Add(obj);
        }

        // Process children recursively
        foreach (Transform child in obj.transform)
        {
            CollectObjectsWithMissingPrefabs(child.gameObject, objectsToProcess);
        }
    }

    static void ReplaceMissingPrefab(GameObject obj)
    {
        // Create a new GameObject to replace the missing prefab
        GameObject newObj = new GameObject(obj.name);

        // Copy the transform properties
        newObj.transform.SetParent(obj.transform.parent);
        newObj.transform.localPosition = obj.transform.localPosition;
        newObj.transform.localRotation = obj.transform.localRotation;
        newObj.transform.localScale = obj.transform.localScale;

        // Copy children to the new GameObject
        List<Transform> children = new List<Transform>();
        foreach (Transform child in obj.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            child.SetParent(newObj.transform);
        }

        // Destroy the original GameObject
        DestroyImmediate(obj);
    }
}
