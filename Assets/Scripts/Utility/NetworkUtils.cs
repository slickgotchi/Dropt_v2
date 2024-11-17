using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using Unity.Netcode;

namespace Dropt.Utils
{
    public static class Network
    {
        public static ulong GetClientIdFromPlayer(NetworkManager networkManager, NetworkObject playerNetworkObject)
        {
            // Check if the network manager or player network object is valid
            if (networkManager == null || playerNetworkObject == null || !playerNetworkObject.HasComponent<PlayerController>())
            {
                Debug.LogWarning("The provided NetworkManager or player NetworkObject is null.");
                return 0; // Or handle appropriately, such as returning an invalid ID
            }

            // Iterate over connected clients to find the one with the matching PlayerObject
            foreach (var client in networkManager.ConnectedClients)
            {
                if (client.Value.PlayerObject == playerNetworkObject)
                {
                    return client.Key; // The LocalClientId of the client
                }
            }

            Debug.LogWarning("Client ID not found for the given player NetworkObject.");
            return 0; // Or handle appropriately
        }
    }
}
