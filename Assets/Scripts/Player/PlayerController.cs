using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] AudioListener playerAudioListener;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            playerAudioListener.enabled = false;
            playerCamera.Priority = 0;
            return;
        }

        playerCamera.Priority = 100;
        playerAudioListener.enabled = true;

        if (IsLocalPlayer && !IsHost)
        {
            PingServerRpc(Time.time);
        }
    }

    [ServerRpc]
    void PingServerRpc(float elapsedTime)
    {
        PongClientRpc(elapsedTime);
    }

    [ClientRpc]
    void PongClientRpc(float elapsedTime)
    {
        if (!IsLocalPlayer) return;

        var rtt = (int)((Time.time - elapsedTime)*1000);
        DebugCanvas.Instance.SetPing(rtt);
        PingServerRpc(Time.time);
    }
}
