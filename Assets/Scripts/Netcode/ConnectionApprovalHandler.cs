using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Connection Approval Handler Component
/// </summary>
/// <remarks>
/// This should be placed on the same GameObject as the NetworkManager.
/// It automatically declines the client connection for example purposes.
/// </remarks>
public class ConnectionApprovalHandler : MonoBehaviour
{
    private NetworkManager m_NetworkManager;

    [SerializeField] private List<Vector3> m_defaulSpawnPositions = new List<Vector3>();

    private void Start()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        if (m_NetworkManager != null)
        {
            m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            m_NetworkManager.ConnectionApprovalCallback = ApprovalCheck;
        }
    }

    Vector3 PopPosition()
    {
        if (m_defaulSpawnPositions.Count <= 0) return Vector3.zero;
        var pos = m_defaulSpawnPositions[0];
        m_defaulSpawnPositions.RemoveAt(0);
        return pos;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //Debug.Log("Approval Check");
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        // get number of players already in game
        int playerCount = Game.Instance.playerControllers.Count;
        Debug.Log("ConnectionApprovalHandler PlayerCount: " + playerCount);

        // Your approval logic determines the following values
        response.Approved = playerCount < 3;
        response.CreatePlayerObject = true;

        // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = PopPosition();

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;

        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = playerCount >= 3 ? "Game at max player capacity of 3" : "";

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }

    private void OnClientConnectedCallback(ulong obj)
    {
        Debug.Log("Client connected, obj: " + obj);
    }

    private void OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log("Client disconnected, obj: " + obj);
    }
}