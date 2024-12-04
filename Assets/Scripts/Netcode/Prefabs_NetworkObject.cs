using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Prefabs_NetworkObject : MonoBehaviour
{
    public static Prefabs_NetworkObject Instance { get; private set; }

    [SerializeField]
    private List<GameObject> networkObjects = new List<GameObject>();

    public List<GameObject> NetworkObjects => networkObjects;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PopulateNetworkObjects(List<GameObject> networkObjectsInProject)
    {
        networkObjects.Clear();
        networkObjects.AddRange(networkObjectsInProject);
    }

    public GameObject GetNetworkObjectByName(string name)
    {
        foreach (GameObject obj in networkObjects)
        {
            if (obj.name == name)
            {
                return obj;
            }
        }
        return null;
    }
}
