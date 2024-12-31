using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeferredSpawner : MonoBehaviour
{
    private static DeferredSpawner _instance;
    private Queue<Action> _nextFrameActions = new Queue<Action>();

    // Singleton pattern to ensure only one instance of the DeferredSpawner exists
    public static DeferredSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("DeferredSpawner");
                _instance = go.AddComponent<DeferredSpawner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // Enqueue the Spawn function to be called on the next frame
    public static void SpawnNextFrame(NetworkObject networkObject)
    {
        if (networkObject == null || Instance == null)
        {
            Debug.LogWarning("Passed a null object (or Instance is null) to SpawnNextFrame");
            return;
        }

        Instance._nextFrameActions.Enqueue(() => {
            if (networkObject == null) return;

            networkObject.gameObject.SetActive(true);
            networkObject.Spawn();
            });
    }

    // Enqueue any action to be performed on the next frame
    public static void CallNextFrame(Action action)
    {
        Instance._nextFrameActions.Enqueue(action);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!NetworkManager.Singleton ||
            NetworkManager.Singleton.ShutdownInProgress ||
            !NetworkManager.Singleton.IsListening) return;

        while (_nextFrameActions.Count > 0)
        {
            // Dequeue and execute all actions that were queued for the next frame
            var action = _nextFrameActions.Dequeue();
            action?.Invoke();
        }
    }
}
