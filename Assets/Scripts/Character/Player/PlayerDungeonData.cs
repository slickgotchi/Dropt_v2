using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDungeonData : NetworkBehaviour
{
    // Public properties with private setters
    public NetworkVariable<int> GltrCount = new NetworkVariable<int>(0);
    public NetworkVariable<int> cGHSTCount = new NetworkVariable<int>(0);
    public NetworkVariable<float> Essence = new NetworkVariable<float>(300);

    private void Update()
    {
        if (!IsServer) return;

        Essence.Value -= Time.deltaTime;

        if (Essence.Value <= 0)
        {
            ShowREKTScreenClientRpc(GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ShowREKTScreenClientRpc(ulong playerNetworkObjectId)
    {
        var player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        var localId = GetComponent<NetworkObject>().NetworkObjectId;
        if (player.NetworkObjectId != localId) return;

        GetComponent<PlayerPrediction>().IsInputDisabled = true;
        REKTCanvas.Instance.Show(REKTCanvas.TypeOfREKT.Essence);
    }

    // Method to add value to GltrCount
    public void AddGltr(int value)
    {
        if (!IsServer) return;

        GltrCount.Value += value;
    }

    // Method to add value to CGHSTCount
    public void AddCGHST(int value)
    {
        if (!IsServer) return;

        cGHSTCount.Value += value;
    }

    public void AddEssence(float value)
    {
        if (!IsServer) return;

        Essence.Value += value;
    }

    // Method to reset counts
    public void Reset()
    {
        if (!IsServer) return;

        GltrCount.Value = 0;
        cGHSTCount.Value = 0;
        Essence.Value = 300;
    }
}
