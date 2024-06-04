using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;

public class DebugCanvas : NetworkBehaviour
{
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI pingText;
    public TextMeshProUGUI playerCountText;

    private float deltaTime = 0.0f;

    private NetworkVariable<int> playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    private NetworkVariable<int> rtt = new NetworkVariable<int>(0);

    private void Update()
    {
        // Update FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();

        Ping();

        PlayerCount();
    }

    void Ping()
    {
        if (!IsOwner) return;

        var clientId = NetworkManager.ServerClientId;
        var ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(clientId);

        pingText.text = "Ping: " + ping.ToString();
    }

    void PlayerCount()
    {
        playerCountText.text = "Players: " + playerCount.Value.ToString();

        if (!IsServer) return;
        playerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;

    }
}
