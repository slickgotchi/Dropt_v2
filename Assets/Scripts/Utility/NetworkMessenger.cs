using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkMessenger : NetworkBehaviour
{
    public static NetworkMessenger Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetGameIsPublic(bool isPublic)
    {
        if (Bootstrap.IsClient())
        {
            Debug.Log("Sending a message to server to set isPublic to " + isPublic);
            SetGameIsPublicServerRpc(isPublic);
        }
    }

    [Rpc(SendTo.Server)]
    void SetGameIsPublicServerRpc(bool isPublic)
    {
        Debug.Log("On server isPublic set to: " + isPublic);
        GameServerHeartbeat.Instance.IsPublic = isPublic;
    }
}
