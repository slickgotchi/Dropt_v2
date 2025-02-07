using Cinemachine;
using PortalDefender.AavegotchiKit;
using PortalDefender.AavegotchiKit.GraphQL;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using GotchiHub;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI m_positionText;

    public GameObject ScreenBlockers;
    private GameObject m_cameraFollower;

    private bool m_isPlayerHUDInitialized = false;

    private NetworkCharacter m_networkCharacter;
    private PlayerPrediction m_playerPrediction;

    [HideInInspector] public static float InactiveTimerDuration = 60 * 60;

    [HideInInspector] public bool IsDead = false;

    [HideInInspector] public bool IsInvulnerable { get; private set; }

    private float m_inactiveTimer = InactiveTimerDuration;

    private HoldBarCanvas m_holdBarCanvas;

    private AttackCentre m_playerAttackCentre;

    // tracking selected gotchi
    private int m_selectedGotchiId = 0;

    [HideInInspector] public bool isGameOvered = false;

    // variables for tracking current gotchi
    private int m_localGotchiId = 0;
    [HideInInspector] public NetworkVariable<int> NetworkGotchiId = new NetworkVariable<int>(69420);
    [HideInInspector] public NetworkVariable<int> m_totalKilledEnemies = new NetworkVariable<int>(0);
    [HideInInspector] public NetworkVariable<int> m_totalDestroyedDestructibles = new NetworkVariable<int>(0);

    private Vector3 m_spawnPoint;

    public Collider2D HurtCollider2D;

    private PlayerCamera m_playerCamera;

    private void Awake()
    {
        m_networkCharacter = GetComponent<NetworkCharacter>();
        m_holdBarCanvas = GetComponentInChildren<HoldBarCanvas>();
        m_playerPrediction = GetComponent<PlayerPrediction>();
        m_playerAttackCentre = GetComponentInChildren<AttackCentre>();
    }

  

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // register player controller
        Game.Instance.playerControllers.Add(GetComponent<PlayerController>());

        // local player
        if (IsLocalPlayer)
        {
            m_selectedGotchiId = 0;
            m_localGotchiId = 0;
            int gotchiId = 0;
            if (PlayerPrefs.HasKey("GotchiId"))
            {
                gotchiId = PlayerPrefs.GetInt("GotchiId");
            }
            else if (Bootstrap.Instance.TestBlockChainGotchiId > 0)
            {
                gotchiId = Bootstrap.Instance.TestBlockChainGotchiId;
            }

            GotchiDataManager.Instance.SetSelectedGotchiById(gotchiId);

            // setup player hud
            if (!m_isPlayerHUDInitialized && GetComponent<NetworkCharacter>() != null)
            {
                PlayerHUDCanvas.Instance.SetLocalPlayerCharacter(GetComponent<PlayerCharacter>());
            }

            m_cameraFollower = GameObject.FindGameObjectWithTag("CameraFollower");
            if (m_cameraFollower != null)
            {
                Debug.Log("Found a camera follower");
                m_cameraFollower.GetComponent<CameraFollowerAndPlayerInteractor>().Player = gameObject;
            }
            

            m_playerCamera = FindAnyObjectByType<PlayerCamera>();

            LevelManager.Instance.OnLevelChangeHeadsUp += Handle_LevelChangeHeadsUp;
            LevelManager.Instance.OnLevelChanged += Handle_LevelChanged;

            m_playerCamera.SnapToTrackedObjectImmediately();
        }

        if (IsClient)
        {
            if (!IsLocalPlayer) ScreenBlockers.SetActive(false);
        }

        if (IsServer)
        {
            ScreenBlockers.SetActive(false);
            m_playerPrediction.SetPlayerPosition(LevelManager.Instance.GetPlayerSpawnPosition());
        }

        // call handle level changed right away because the server has
        // already sent messages about level changes that we would not be aware of
        if (!IsHost) _ = StartupLevelChanged();
    }

    async UniTaskVoid StartupLevelChanged()
    {
        await UniTask.Delay(1000);
        Handle_LevelChanged(Level.NetworkLevel.LevelType.Null, Level.NetworkLevel.LevelType.Null);
    }

    public override void OnNetworkDespawn()
    {

        // degregister player controller
        Game.Instance.playerControllers.Remove(GetComponent<PlayerController>());

        if (!IsServer) return;

        // check for any pets owned
        if (Game.Instance != null)
        {
            var pets = Game.Instance.petControllers;
            if (pets.Count > 0)
            {
                var playerNetworkObjectId = GetComponent<NetworkObject>().NetworkObjectId;

                foreach (var pet in pets)
                {
                    if (pet.GetPetOwnerNetworkObjectId() == playerNetworkObjectId)
                    {
                        pet.GetComponent<NetworkObject>().Despawn();
                    }
                }
            }
        }

        if (IsLocalPlayer)
        {
            LevelManager.Instance.OnLevelChangeHeadsUp -= Handle_LevelChangeHeadsUp;
            LevelManager.Instance.OnLevelChanged -= Handle_LevelChanged;
        }


        base.OnNetworkDespawn();

    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            if (m_positionText != null)
            {
                // update position text
                var pos = transform.position;
                m_positionText.text = $"({pos.x:F2}, {pos.y:F2})";
            }

            if (m_cameraFollower != null)
            {
                m_cameraFollower.transform.position = m_playerPrediction.GetLocalPlayerInterpPosition() + new Vector3(0, 0.5f, 0f);
            }

            if (m_playerCamera == null)
            {
                m_playerCamera = FindAnyObjectByType<PlayerCamera>();
            }

            // check for player input to ensure we stay active
            CheckForPlayerInput();

            if (Input.GetKeyDown(KeyCode.T))
            {
                if (m_playerCamera != null)
                {
                    m_playerCamera.Shake();
                }
                else
                {
                    Debug.LogWarning("Don't have a PlayerCamera");
                }
            }
        }

        if (IsHost || (IsClient && Bootstrap.IsLocalConnection()))
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                GetComponent<NetworkCharacter>().TakeDamage(300, false);
            }
        }

        HandleDegenapeResetKillAndDestructibleCount();
        HandleDegenapeHpAp();
        HandleInactivePlayer();

        // handle gotchi changes
        HandleLocalGotchiIdChanges();
        HandleRemoteGotchiIdChanges();

        // keep the hold bar in position (webgl it is weirdly in wrong position);
        if (m_holdBarCanvas != null) m_holdBarCanvas.transform.localPosition = new Vector3(0, 2, 0);
    }

    // this is where we set everything required for leaderboarding
    [Rpc(SendTo.Server)]
    private void SetNetworkGotchiIdServerRpc(int gotchiId, string authToken)
    {
        _ = SetNetworkGotchiIdServerRpc_ASYNC(gotchiId, authToken);
    }

    private async UniTaskVoid SetNetworkGotchiIdServerRpc_ASYNC(int gotchiId, string authToken)
    {
        if (!IsServer) return;

        // make sure we're in the Degenape village!
        // players can only change gotchis in the village. this prevents someone using
        // someone elses god gotchi (client side hack) and then switching back to theirs
        if (!LevelManager.Instance.IsDegenapeVillage()) return;

        // if its a default gotchi there is no need for address checking
        if (gotchiId > 25000)
        {
            NetworkGotchiId.Value = gotchiId;
            return;
        }
        else
        {
            // ensure the gotchi specified is actually on the wallet associated with the
            // given authToken
            var addressByToken = await Dropt.Utils.Http.GetAddressByAuthToken(authToken);
            if (!string.IsNullOrEmpty(addressByToken))
            {
                var userData = await GraphManager.Instance.GetUserAccount(addressByToken.ToLower());
                if (userData != null)
                {
                    foreach (var gotchi in userData.gotchisOwned)
                    {
                        if (gotchi.id == gotchiId)
                        {
                            NetworkGotchiId.Value = gotchiId;
                            return;
                        }
                    }
                }
            }
        }
    }

    public async UniTask KillPlayer(REKTCanvas.TypeOfREKT typeOfREKT)
    {
        try
        {
            if (IsDead) return;
            IsDead = true;

            var playerOffchainData = GetComponent<PlayerOffchainData>();
            if (playerOffchainData != null)
            {
                await playerOffchainData.ExitDungeonCalculateBalances(false);
            }

            var playerLeaderboardLogger = GetComponent<PlayerLeaderboardLogger>();
            if (playerLeaderboardLogger != null)
            {
                await LeaderboardLogger.LogEndOfDungeonResults(
                    playerLeaderboardLogger.GetComponent<PlayerController>(),
                    playerLeaderboardLogger.dungeonType,
                    false);
            }

            var networkObject = GetComponent<NetworkObject>();
            if (networkObject == null) { Debug.LogWarning("KillPlayer: networkObject = null"); return; }

            isGameOvered = true;

            TriggerGameOverClientRpc(networkObject.NetworkObjectId, typeOfREKT);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
        if (!IsServer) return;

        
    }

    public void StartInvulnerability(float duration)
    {
        StartInvulnerabilityServerRpc(duration);
    }

    [ServerRpc]
    private void StartInvulnerabilityServerRpc(float duration)
    {
        CancelInvoke();
        IsInvulnerable = true;
        Invoke(nameof(StopInvulnerability), duration);
    }

    private void StopInvulnerability()
    {
        IsInvulnerable = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerGameOverClientRpc(ulong killedPlayerNetworkObjectId, REKTCanvas.TypeOfREKT typeOfREKT)
    {
        if (!IsLocalPlayer) return;

        isGameOvered = true;

        REKTCanvas.Instance.Show(typeOfREKT);
    }


    private bool m_isDoReset = false;
    private float m_resetTimer = 0f;

    Vector3 prevPosition;

    private void HandleLocalGotchiIdChanges()
    {
        if (!IsLocalPlayer) return;

        // handle local player selected gotchi changes
        if (LevelManager.Instance.IsDegenapeVillage())
        {
            var selectedGotchiId = GotchiDataManager.Instance.GetSelectedGotchiId();
            if (selectedGotchiId != m_selectedGotchiId)
            {
                m_selectedGotchiId = selectedGotchiId;

                var authToken = PlayerPrefs.GetString("AuthToken");
                if (string.IsNullOrEmpty(authToken))
                {
                    Debug.LogWarning("Please sign in and obtain an auth token");
                    return;
                }

                SetNetworkGotchiIdServerRpc(m_selectedGotchiId, authToken);
            }
        }
    }

    private void HandleRemoteGotchiIdChanges()
    {
        if (!IsClient) return;

        if (NetworkGotchiId.Value != m_localGotchiId)
        {
            m_localGotchiId = NetworkGotchiId.Value;
            _ = SetupGotchi(m_localGotchiId);
        }
    }

    private async UniTask SetupGotchi(int gotchiId)
    {
        if (!IsClient) return;

        try
        {
            var isFetchSuccess = await GotchiDataManager.Instance.FetchGotchiById(gotchiId);
            if (isFetchSuccess)
            {
                PlayerPrefs.SetInt("GotchiId", gotchiId);
                GetComponent<PlayerSVGs>().Init(gotchiId);
                GetComponent<PlayerEquipment>().Init(gotchiId);
                GetComponent<PlayerCharacter>().Init(gotchiId);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }


    // parameters for handle loading canvas
    //private bool shouldBeBlackedOut = true;
    //private bool isBlackedOut = true;

    void Handle_LevelChangeHeadsUp()
    {
        if (IsLocalPlayer)
        {
            //Debug.Log("WipeIn");
            LoadingCanvas.Instance.WipeIn();
        }

    }

    void Handle_LevelChanged(Level.NetworkLevel.LevelType oldLevel, Level.NetworkLevel.LevelType newLevel)
    {
        if (IsLocalPlayer)
        {
            m_playerCamera.SnapToTrackedObjectImmediately();
            GetComponent<PlayerGotchi>().PlayDropAnimation();

            //Debug.Log("WipeOut");
            LoadingCanvas.Instance.WipeOut();
            GetComponent<PlayerPrediction>().IsInputEnabled = true;
            Debug.Log("InputEnabled");
        }

    }

    private void HandleDegenapeResetKillAndDestructibleCount()
    {
        if (!IsServer) return;

        if (LevelManager.Instance.IsDegenapeVillage())
        {
            m_totalKilledEnemies.Value = 0;
            m_totalDestroyedDestructibles.Value = 0;
        }
    }

    private void HandleDegenapeHpAp()
    {
        if (!IsServer) return;

        if (LevelManager.Instance.IsDegenapeVillage())
        {
            m_networkCharacter.currentDynamicStats.HpCurrent = m_networkCharacter.currentStaticStats.HpMax;
            m_networkCharacter.currentDynamicStats.ApCurrent = m_networkCharacter.currentStaticStats.ApMax;
        }
    }

    private bool m_isActiveInput = false;
    private float m_isActiveInputTimer = 0f;

    private void CheckForPlayerInput()
    {
        m_isActiveInputTimer -= Time.deltaTime;

        // Check for any key or mouse button
        if (Input.anyKey)
        {
            m_isActiveInput = true;
        }

        // Check for any mouse movement
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            m_isActiveInput = true;
        }

        // Check for mouse scroll wheel movement
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            m_isActiveInput = true;
        }

        // Check for any touch input (mobile devices)
        if (Input.touchCount > 0)
        {
            m_isActiveInput = true;
        }

        if (m_isActiveInput && m_isActiveInputTimer < 0)
        {
            m_isActiveInput = false;
            m_isActiveInputTimer = 10;
            ResetInactiveTimerServerRpc();
        }
    }

    // need this so that a player that is not playing gets booted off the server
    private void HandleInactivePlayer()
    {
        if (!IsServer) return;

        m_inactiveTimer -= Time.deltaTime;

        if (m_inactiveTimer <= 0)
        {
            _ = KillPlayer(REKTCanvas.TypeOfREKT.InActive);
        }
    }

    // this gets called by the server side of PlayerPrediction
    [Rpc(SendTo.Server)]
    public void ResetInactiveTimerServerRpc()
    {
        if (!IsServer) return;

        m_inactiveTimer = InactiveTimerDuration;
    }

    public void AddToTotalKilledEnemies()
    {
        m_totalKilledEnemies.Value++;
    }

    public int GetTotalKilledEnemies()
    {
        return m_totalKilledEnemies.Value;
    }

    public void AddToTotalDestroyedDestructibles()
    {
        m_totalDestroyedDestructibles.Value++;
    }

    public int GetTotalDestroyedDestructibles()
    {
        return m_totalDestroyedDestructibles.Value;
    }
}