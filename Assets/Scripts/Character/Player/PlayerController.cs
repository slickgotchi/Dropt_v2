using CarlosLab.UtilityIntelligence;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// PlayerController handles:
// - camera/virtual camera
// - audio listener
// - level spawning

public class PlayerController : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] AudioListener playerAudioListener;

    [SerializeField] TextMeshProUGUI m_positionText;

    private bool m_isPlayerHUDInitialized = false;

    public GameObject TestFernPrefab;

    private NetworkCharacter m_networkCharacter;

    public static float InactiveTimerDuration = 5 * 60;

    public bool IsDead = false;

    private float m_inactiveTimer = InactiveTimerDuration;

    private void Awake()
    {
        m_networkCharacter = GetComponent<NetworkCharacter>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer) {
            playerCamera.Priority = 100;
            playerAudioListener.enabled = true;

            // Get the currently active scene to ensure we're working with the "Game" scene
            Scene gameScene = SceneManager.GetSceneByName("Game");

            // change all canvas to camera mode and set players camera to them
            var canvii = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvii)
            {
                // Check if the Canvas is in the "Game" scene and has a parent named "Game"
                //if (canvas.gameObject.scene == gameScene)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = GetComponentInChildren<Camera>();
                }
            }
        } else
        {
            playerAudioListener.enabled = false;
            playerCamera.Priority = 0;
        }

        if (IsServer && !IsHost)
        {
            //PingServerRpc(Time.time);
        }

        
    }

    public void KillPlayer(REKTCanvas.TypeOfREKT typeOfREKT)
    {
        if (!IsServer) return;

        GetComponent<PlayerController>().IsDead = true;
        TriggerGameOverClientRpc(GetComponent<NetworkObject>().NetworkObjectId, typeOfREKT);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void TriggerGameOverClientRpc(ulong playerNetworkObjectId, REKTCanvas.TypeOfREKT typeOfREKT)
    {
         //ensure we only trigger this for the relevant player
        var player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        var localId = GetComponent<NetworkObject>().NetworkObjectId;
        if (player.NetworkObjectId != localId) return;

        Game.Instance.TriggerGameOver(typeOfREKT);
    }

    public void StartTransition()
    {
        if (!IsServer) return;
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            if (m_positionText != null)
            {
                // update position text
                var pos = transform.position;
                m_positionText.text = $"({pos.x.ToString("F2")}, {pos.y.ToString("F2")})";
            }

            HandleLevelManagerState();

            // setup player hud
            if (!m_isPlayerHUDInitialized && GetComponent<NetworkCharacter>() != null)
            {
                PlayerHUDCanvas.Singleton.SetLocalPlayerCharacter(GetComponent<NetworkCharacter>());
            }
        }

        HandleNextLevelCheat();
        HandleSpawnFern();
        HandleDegenapeHpAp();
        HandleInactivePlayer();
    }

    private LevelManager.TransitionState m_localTransition = LevelManager.TransitionState.Null;

    void HandleLevelManagerState()
    {
        if (!IsLocalPlayer) return;

        LevelManager.TransitionState state = LevelManager.Instance.State.Value;

        if (state == LevelManager.TransitionState.Start ||
            state == LevelManager.TransitionState.ClientHeadsUp || 
            state == LevelManager.TransitionState.GoToNext)
        {
            if (m_localTransition != LevelManager.TransitionState.ClientHeadsUp)
            {
                m_localTransition = LevelManager.TransitionState.ClientHeadsUp;
                LoadingCanvas.Instance.Animator.Play("LoadingCanvas_WipeIn");
            }
        }

        if (state == LevelManager.TransitionState.ClientHeadsDown ||
            state == LevelManager.TransitionState.End ||
            state == LevelManager.TransitionState.Null)
        {
            if (m_localTransition == LevelManager.TransitionState.ClientHeadsUp)
            {
                m_localTransition = LevelManager.TransitionState.Null;
                LoadingCanvas.Instance.Animator.Play("LoadingCanvas_WipeOut");
                REKTCanvas.Instance.Container.SetActive(false);
            }
        }
    }


    // cheat to go to next level
    void HandleNextLevelCheat()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.N))
        {
            GoNextLevelServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void GoNextLevelServerRpc()
    {
        LevelManager.Instance.GoToNextLevel();
    }

    // cheat to go to next level
    void HandleSpawnFern()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            SpawnFernServerRpc();
        }
    }

    void HandleDegenapeHpAp()
    {
        if (!IsServer) return;

        if (LevelManager.Instance.CurrentLevelIndex.Value == LevelManager.Instance.DegenapeVillageLevel)
        {
            m_networkCharacter.HpCurrent.Value = m_networkCharacter.HpMax.Value;
            m_networkCharacter.ApCurrent.Value = m_networkCharacter.ApMax.Value;
        }
    }

    // need this so that a player that is not playing gets booted off the server
    void HandleInactivePlayer()
    {
        if (!IsServer) return;

        m_inactiveTimer -= Time.deltaTime;

        if (UnityEngine.Input.anyKey)
        {
            m_inactiveTimer = InactiveTimerDuration;
        }

        if (m_inactiveTimer <= 0)
        {
            KillPlayer(REKTCanvas.TypeOfREKT.InActive);
        }
    }

    [Rpc(SendTo.Server)]
    void SpawnFernServerRpc()
    {
        var obj = Instantiate(TestFernPrefab);
        obj.transform.position = transform.position;
        obj.GetComponent<NetworkObject>().Spawn();
    }
}
