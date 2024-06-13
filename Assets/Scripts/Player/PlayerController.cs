using CarlosLab.UtilityIntelligence;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

// PlayerController handles:
// - camera/virtual camera
// - audio listener
// - level spawning

public class PlayerController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] AudioListener playerAudioListener;



    [HideInInspector] public bool isTryToSpawn = false;
    [HideInInspector] public bool isSpawned = false;

    [SerializeField] TextMeshProUGUI m_positionText;

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

        if (IsServer && !IsHost)
        {
            PingServerRpc(Time.time);
        }

        // set the player to an available spawn point
        TryGetNewSpawnPoint();
    }

    public void TryGetNewSpawnPoint()
    {
        if (!IsServer) return;

        isTryToSpawn = true;
        isSpawned = false;
    }

    public override void OnNetworkDespawn()
    {

    }

    private void Update()
    {
        if (IsServer && isTryToSpawn && !isSpawned && 
            LevelManager.Instance != null && LevelManager.Instance.IsLevelCreated())
        {
            var spawnPoint = LevelManager.Instance.PopPlayerSpawnPoint();
            Debug.Log("Spawn player at: " + spawnPoint);
            var currentPosition = transform.position;

            GetComponent<PlayerMovementAndDash>().SetPlayerPosition(spawnPoint);
            GetComponent<PlayerGotchi>().DropSpawn(currentPosition, spawnPoint);
            isSpawned = true;
            isTryToSpawn = false;
        }

        if (IsLocalPlayer)
        {
            // update position text
            var pos = transform.position;
            m_positionText.text = $"({pos.x.ToString("F2")}, {pos.y.ToString("F2")})";
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

        var rtt = (int)((Time.time - elapsedTime) * 1000);
        DebugCanvas.Instance.SetPing(rtt);
        PingServerRpc(Time.time);
    }
}
