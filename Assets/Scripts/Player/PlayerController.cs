using CarlosLab.UtilityIntelligence;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] AudioListener playerAudioListener;

    private UtilityEntityController m_entityController;

    public bool isTryToSpawn = false;
    public bool isSpawned = false;

    private void Awake()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer) {
            playerCamera.Priority = 100;
            playerAudioListener.enabled = true;
        } else
        {
            playerAudioListener.enabled = false;
            playerCamera.Priority = 0;
        }

        m_entityController = GetComponent<UtilityEntityController>();

        // register utility entity if this is the server instance
        if (IsServer)
        {
            m_entityController.Register(UtilityWorldSingleton.Instance.World);
        } else if (!IsHost)
        {
            GetComponent<PlayerEntityFacade>().enabled = false;
            m_entityController.enabled = false;
        }

        if (IsServer && !IsHost)
        {
            PingServerRpc(Time.time);
        }

        // set the player to an available spawn point
        if (IsServer)
        {
            isTryToSpawn = true;
        }
    }

    public override void OnNetworkDespawn()
    {
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

    private void Update()
    {
        if (IsServer && isTryToSpawn && !isSpawned && 
            LevelManager.Instance != null && LevelManager.Instance.IsLevelCreated())
        {
            var spawnPoint = LevelManager.Instance.GetPlayerSpawnPoint();
            GetComponent<PlayerMovementAndDash>().SetPlayerPosition(spawnPoint);
            GetComponent<PlayerGotchi>().DropSpawn(spawnPoint);
            isSpawned = true;
        }
    }
}
