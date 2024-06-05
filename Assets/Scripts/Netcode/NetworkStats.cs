using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkStats : NetworkBehaviour
{
    public static NetworkStats Instance { get; private set; }

    public float FPS = 0;
    public int Ping = 0;
    public int ConnectedPlayers = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        FPS = 1.0f / Time.deltaTime;

        UpdatePing();

        UpdatePlayerCount();
    }

    void UpdatePing()
    {
        if (!IsOwner) return;

        var clientId = NetworkManager.ServerClientId;
        Ping = (int)NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(clientId);
    }

    void UpdatePlayerCount()
    {
        if (!IsServer) return;
        ConnectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
