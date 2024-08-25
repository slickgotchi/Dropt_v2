using CarlosLab.UtilityIntelligence;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using GotchiHub;

public class PlayerController : NetworkBehaviour
{
    public GameObject ScreenBlockers;

    private GameObject m_cameraFollower;
    private PlayerPrediction m_playerPrediction;

    [SerializeField] TextMeshProUGUI m_positionText;

    private bool m_isPlayerHUDInitialized = false;

    private NetworkCharacter m_networkCharacter;

    public static float InactiveTimerDuration = 2 * 60;

    public bool IsDead = false;

    private float m_inactiveTimer = InactiveTimerDuration;

    private HoldBarCanvas m_holdBarCanvas;

    private void Awake()
    {
        m_networkCharacter = GetComponent<NetworkCharacter>();
        m_holdBarCanvas = GetComponentInChildren<HoldBarCanvas>();
        m_playerPrediction = GetComponent<PlayerPrediction>();

        GotchiDataManager.Instance.onSelectedGotchi += HandleOnSelectedGotchi;
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer) {
            Debug.Log("Set camera follower");
            m_cameraFollower = GameObject.FindGameObjectWithTag("CameraFollower");
        } else
        {
            ScreenBlockers.SetActive(false);
        }

        // if in host mode, set a starting gotchi id
        int gotchiId;
        if (PlayerPrefs.HasKey("GotchiId"))
        {
            gotchiId = PlayerPrefs.GetInt("GotchiId");
        }
        else if (Bootstrap.Instance.TestBlockChainGotchiId > 0)
        {
            gotchiId = Bootstrap.Instance.TestBlockChainGotchiId;
        }
        else
        {
            return;
        }

        // setup gotchi if we got a valid id
        SetupGotchi(gotchiId);
    }

    private async void SetupGotchi(int gotchiId)
    {
        if (!IsLocalPlayer) return;

        try
        {
            var isFetchSuccess = await GotchiDataManager.Instance.FetchGotchiById(gotchiId);

            if (isFetchSuccess)
            {
                GetComponent<PlayerSVGs>().Init(gotchiId);
                GetComponent<PlayerEquipment>().Init(gotchiId);
                GetComponent<PlayerCharacter>().InitWearableBuffs(gotchiId);
                GetComponent<PlayerCharacter>().InitGotchiStats(gotchiId);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }

    void HandleOnSelectedGotchi(int id)
    {
        if (!IsLocalPlayer) return;

        SetupGotchi(id);
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

            // set camera to follow player
            if (m_cameraFollower != null)
            {
                m_cameraFollower.transform.position = m_playerPrediction.GetLocalPlayerInterpPosition();
            }
        }

        HandleNextLevelCheat();
        HandleDegenapeHpAp();
        HandleInactivePlayer();

        // keep the hold bar in position (webgl it is weirdly in wrong position);
        if (m_holdBarCanvas != null) m_holdBarCanvas.transform.localPosition = new Vector3(0, 2, 0);
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

        if (m_inactiveTimer <= 0)
        {
            KillPlayer(REKTCanvas.TypeOfREKT.InActive);
        }
    }

    // this gets called by the server side of PlayerPrediction
    public void ResetInactiveTimer()
    {
        if (!IsServer) return;

        m_inactiveTimer = InactiveTimerDuration;
    }
}
