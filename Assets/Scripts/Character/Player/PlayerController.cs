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

    public GameObject TestFernPrefab;

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
            m_cameraFollower = GameObject.FindGameObjectWithTag("CameraFollower");
        } else
        {
            ScreenBlockers.SetActive(false);
        }

        // if gotchi id not 0, try get the gotchi data manager to add this gotchi
        //if (Game.Instance.LocalGotchiId != 0)
        //{
        //    GetComponent<PlayerSVGs>().UpdateGotchiIdServerRpc(Game.Instance.LocalGotchiId);
        //    GetComponent<PlayerEquipment>().SetPlayerEquipmentByGotchiId(Game.Instance.LocalGotchiId);
        //}


        // if player spawns and is not local, let them decide their gotchi id from their local instance, not here
        if (!IsLocalPlayer)
        {
            // Do nothing
        } else if (IsLocalPlayer)
        {
            // check player prefs for last used gotchi id
            if (PlayerPrefs.HasKey("GotchiId"))
            {
                var gotchiId = PlayerPrefs.GetInt("GotchiId");

                // set player svg to the stored gotchi id
                GetComponent<PlayerSVGs>().UpdateGotchiIdServerRpc(gotchiId);

                // set equipment to the stored gotchi id
                GetComponent<PlayerEquipment>().SetPlayerEquipmentByGotchiId(gotchiId);
            }
            
        }
    }

    void HandleOnSelectedGotchi(int id)
    {
        if (!IsLocalPlayer) return;

        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);

        // update character stats
        var hp = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[0], TraitType.NRG);
        var attack = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[1], TraitType.AGG);
        var critChance = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[2], TraitType.SPK);
        var ap = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[3], TraitType.BRN);
        var doubleStrikeChance = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[4], TraitType.EYS);
        var critDamage = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[5], TraitType.EYC);

        SetBaseStatsServerRpc(hp, attack, critChance, ap, doubleStrikeChance, critDamage);

        // save our selected gotchi into our player prefs
        PlayerPrefs.SetInt("GotchiId", gotchiData.id);
    }

    //void SetBaseStats(float hp, float atk, float critChance, float ap, float doubleStrike, float critDamage)
    //{
    //    SetBaseStatsServerRpc(hp, atk, critChance, ap, doubleStrike, critDamage);
    //}

    [Rpc(SendTo.Server)]
    void SetBaseStatsServerRpc(float hp, float atk, float critChance, float ap, float doubleStrike, float critDamage)
    {
        var networkCharacter = GetComponent<NetworkCharacter>();

        networkCharacter.HpMax.Value = (int)hp;
        networkCharacter.HpCurrent.Value = (int)hp;
        networkCharacter.HpBuffer.Value = 0;

        networkCharacter.AttackPower.Value = (int)atk;

        networkCharacter.CriticalChance.Value = critChance;

        networkCharacter.ApMax.Value = (int)ap;
        networkCharacter.ApCurrent.Value = (int)ap;
        networkCharacter.ApBuffer.Value = 0;

        networkCharacter.DoubleStrikeChance.Value = doubleStrike;

        networkCharacter.CriticalDamage.Value = critDamage;

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
        HandleSpawnFern();
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

    [Rpc(SendTo.Server)]
    void SpawnFernServerRpc()
    {
        var obj = Instantiate(TestFernPrefab);
        obj.transform.position = transform.position;
        obj.GetComponent<NetworkObject>().Spawn();
    }
}
