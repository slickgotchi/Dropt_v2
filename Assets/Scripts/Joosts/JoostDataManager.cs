using UnityEngine;
using System.Collections.Generic;

public class JoostDataManager : MonoBehaviour
{
    public static JoostDataManager Instance { get; private set; }

    private Dictionary<Joost.Type, JoostObject> joostDataDictionary;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadJoostData();
    }

    private void LoadJoostData()
    {
        joostDataDictionary = new Dictionary<Joost.Type, JoostObject>();
        JoostObject[] joostObjects = Resources.LoadAll<JoostObject>("Joosts");

        if (joostObjects.Length == 0)
        {
            Debug.LogError("No JoostObjects found in Resources/Joosts!");
            return;
        }

        foreach (var joostObject in joostObjects)
        {
            if (joostObject == null)
            {
                Debug.LogError("Found a null JoostObject!");
                continue;
            }

            if (!joostDataDictionary.ContainsKey(joostObject.joostType))
            {
                joostDataDictionary.Add(joostObject.joostType, joostObject);
                //Debug.Log($"Added {joostObject.joostType} to the dictionary.");
            }
            else
            {
                Debug.LogWarning($"Duplicate JoostObject found for type: {joostObject.joostType}. Only the first one will be used.");
            }
        }
    }

    public JoostObject GetJoostData(Joost.Type joostType)
    {
        if (joostDataDictionary == null)
        {
            Debug.LogError("JoostDataDictionary is not initialized.");
            return null;
        }

        if (joostDataDictionary.TryGetValue(joostType, out JoostObject joostObject))
        {
            //Debug.Log($"JoostObject of type {joostType} found.");
            return joostObject;
        }
        else
        {
            Debug.LogWarning($"JoostObject of type {joostType} not found.");
            return null;
        }
    }
}
