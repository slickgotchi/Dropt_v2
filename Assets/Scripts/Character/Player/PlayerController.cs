using Cinemachine;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using GotchiHub;
using Cysharp.Threading.Tasks;

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

    [HideInInspector] public bool IsLevelSpawnPositionSet = true;

    private AttackCentre m_playerAttackCentre;

    // tracking selected gotchi
    private int m_selectedGotchiId = 0;

    [HideInInspector] public bool isGameOvered = false;

    // variables for tracking current gotchi
    private int m_localGotchiId = 0;
    [HideInInspector] public NetworkVariable<int> NetworkGotchiId = new NetworkVariable<int>(69420);
    [HideInInspector] public NetworkVariable<int> m_totalKilledEnemies = new NetworkVariable<int>(0);
    [HideInInspector] public NetworkVariable<int> m_totalDestroyedDestructibles = new NetworkVariable<int>(0);

    private CinemachineVirtualCamera m_virtualCamera;

    private Vector3 m_spawnPoint;




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

        IsLevelSpawnPositionSet = true;

        if (IsClient) Application.focusChanged += OnApplicationFocusChanged;

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

            var virtualCameraGameObject = GameObject.FindGameObjectWithTag("VirtualCamera");
            if (virtualCameraGameObject == null)
            {
                Debug.LogWarning("No virtual camera exists in the scene");
                return;
            }

            m_virtualCamera = virtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
        }


        if (!IsLocalPlayer)
        {
            ScreenBlockers.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient) Application.focusChanged -= OnApplicationFocusChanged;

        // degregister player controller
        Game.Instance.playerControllers.Remove(GetComponent<PlayerController>());

        if (!IsServer) return;

        // check for any pets owned
        var pets = Game.Instance.petControllers;
        var playerNetworkObjectId = GetComponent<NetworkObject>().NetworkObjectId;

        foreach (var pet in pets)
        {
            if (pet.GetPetOwnerNetworkObjectId() == playerNetworkObjectId)
            {
                pet.GetComponent<NetworkObject>().Despawn();
            }
        }



        base.OnNetworkDespawn();

    }

    [Rpc(SendTo.Server)]
    void ValidateVersionServerRpc()
    {

    }

    [Rpc(SendTo.Server)]
    void ValidateVersionClientRpc(string serverVersion)
    {
        if (!IsLocalPlayer) return;

        if (serverVersion != Bootstrap.Instance.Version)
        {
            ErrorDialogCanvas.Instance.Show("Your local browser version of Dropt does not match the server. Please hard refresh your browser to update.");
            Debug.Log("Shutdown NetworkManager");
            NetworkManager.Singleton.Shutdown();
        }
    }

    private System.DateTime? m_focusLostTimestamp = null;

    void OnApplicationFocusChanged(bool hasFocus)
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        if (!hasFocus)
        {
            // Record the timestamp when focus is lost
            m_focusLostTimestamp = System.DateTime.UtcNow;
            Debug.Log("Lost focus, taking a timestamp");
        }
        else
        {
            // When focus is regained, check the elapsed time
            if (m_focusLostTimestamp.HasValue)
            {
                double elapsedMilliseconds = (System.DateTime.UtcNow - m_focusLostTimestamp.Value).TotalMilliseconds;

                if (elapsedMilliseconds > unityTransport.DisconnectTimeoutMS)
                {
                    Debug.Log("You were disconnected due to inactivity while the game was unfocused.");
                    ErrorDialogCanvas.Instance.Show("Disconnected from the server due to being out of focus (tabbed out) longer than " + unityTransport.DisconnectTimeoutMS + "s. Please refresh for a new game!");
                }
                else
                {
                    Debug.Log("Welcome back! No disconnection occurred.");
                }

                // Reset the timestamp
                m_focusLostTimestamp = null;
            }
        }
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

            HandleLevelTransition();

            // setup player hud
            if (!m_isPlayerHUDInitialized && GetComponent<NetworkCharacter>() != null)
            {
                PlayerHUDCanvas.Instance.SetLocalPlayerCharacter(GetComponent<PlayerCharacter>());
            }

            // Set camera to follow player (if it exists)
            if (m_cameraFollower == null)
            {
                m_cameraFollower = GameObject.FindGameObjectWithTag("CameraFollower");
                if (m_cameraFollower != null)
                {
                    m_cameraFollower.GetComponent<CameraFollowerAndPlayerInteractor>().Player = gameObject;
                }
            }
            else
            {
                m_cameraFollower.transform.position = m_playerPrediction.GetLocalPlayerInterpPosition() + new Vector3(0, 0.5f, 0f);
            }


            HandleNextLevelCheat();

            // check for player input to ensure we stay active
            CheckForPlayerInput();
        }

        if (IsHost)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                GetComponent<NetworkCharacter>().TakeDamage(300, false);
            }
        }

        // Handle level spawning on the server
        //if (IsServer && !IsLevelSpawnPositionSet)
        if (IsServer && LevelManager.Instance.isPlayersSpawnable &&
            !LevelManager.Instance.spawnedPlayers.Contains(this))
        {
            var pos = LevelManager.Instance.TryGetPlayerSpawnPoint();
            Debug.Log("Spawn Player at " + pos);
            if (pos != null)
            {
                var spawnPoint = (Vector3)pos;
                GetComponent<PlayerPrediction>().SetPlayerPosition(spawnPoint);

                // Position the camera follower directly at the spawn point
                SetCameraPositionClientRpc(spawnPoint + new Vector3(0, 0.5f, 0), GetComponent<NetworkObject>().NetworkObjectId);

                // Call DropSpawn to perform any additional logic
                GetComponent<PlayerGotchi>().PlayDropAnimation();

                // Mark the spawn position as set
                IsLevelSpawnPositionSet = true;
                m_spawnPoint = spawnPoint;

                LevelManager.Instance.spawnedPlayers.Add(this);
            }
        }

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
    public void SetNetworkGotchiIdServerRpc(int gotchiId)
    {
        // make sure we're in the Degenape village!
        // players can only change gotchis in the village. this prevents someone using
        // someone elses god gotchi (client side hack) and then switching back to theirs
        if (!LevelManager.Instance.IsDegenapeVillage()) return;

        NetworkGotchiId.Value = gotchiId;
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
                _ = LeaderboardLogger.LogEndOfDungeonResults(
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
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject == null) { Debug.LogWarning("networkObject = null"); return; }

        if (networkObject.NetworkObjectId != killedPlayerNetworkObjectId) return;

        isGameOvered = true;

        REKTCanvas.Instance.Show(typeOfREKT);
    }

    public void StartTransition()
    {
        if (!IsServer) return;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetCameraPositionClientRpc(Vector3 position, ulong networkObjectId)
    {
        if (GetComponent<NetworkObject>().NetworkObjectId != networkObjectId) return;
        if (m_cameraFollower == null) return;

        var delta = position - m_cameraFollower.transform.position;
        m_cameraFollower.transform.position = position;

        // Temporarily set damping to zero for instant teleport
        var framingTransposer = m_virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        //Debug.Log("transposer: " + framingTransposer);
        float originalDamping = framingTransposer.m_XDamping; // Store original damping
        framingTransposer.m_XDamping = 0;
        framingTransposer.m_YDamping = 0;
        framingTransposer.m_ZDamping = 0;

        m_virtualCamera.OnTargetObjectWarped(m_cameraFollower.transform, delta);

        m_isDoReset = true;
        m_resetTimer = 1f;
    }

    private bool m_isDoReset = false;
    private float m_resetTimer = 0f;

    private void LateUpdate()
    {
        m_resetTimer -= Time.deltaTime;

        if (m_isDoReset && m_resetTimer < 0)
        {
            m_isDoReset = false;

            var framingTransposer = m_virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            float originalDamping = framingTransposer.m_XDamping; // Store original damping
            framingTransposer.m_XDamping = 1;
            framingTransposer.m_YDamping = 1;
            framingTransposer.m_ZDamping = 0;
        }

        // track our speed
        var displacement = transform.position - prevPosition;
        prevPosition = transform.position;
        var distance = displacement.magnitude;
        var velocity = distance / Time.deltaTime;
    }

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
                SetNetworkGotchiIdServerRpc(m_selectedGotchiId);
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


    private enum LoadingCanvasState { Null, BlackOut, WipeIn, WipeOut }
    private LoadingCanvasState loadingCanvasState = LoadingCanvasState.Null;

    // handle loading canvas
    private void HandleLevelTransition()
    {
        if (!IsLocalPlayer) return;
        if (LevelManager.Instance == null) return;


        LevelManager.TransitionState state = LevelManager.Instance.transitionState.Value;

        if (state == LevelManager.TransitionState.Start ||
            state == LevelManager.TransitionState.ClientHeadsUp ||
            state == LevelManager.TransitionState.GoToNext ||
            state == LevelManager.TransitionState.ClientHeadsDown)
        {
            LoadingCanvas.Instance.WipeIn();

            // disable player input
            GetComponent<PlayerPrediction>().IsInputEnabled = false;
        }
        else if (state == LevelManager.TransitionState.End)
        {
            LoadingCanvas.Instance.WipeOut();
        }
        else
        {
            LoadingCanvas.Instance.InstaClear();
        }
    }

    // cheat to go to next level
    private void HandleNextLevelCheat()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoNextLevelServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            GoToDegenapeVillageServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void GoNextLevelServerRpc()
    {
        LevelManager.Instance.StartTransitionToNextLevel_SERVER();
    }

    [Rpc(SendTo.Server)]
    private void GoToDegenapeVillageServerRpc()
    {
        LevelManager.Instance.GoToDegenapeVillageLevel_SERVER();
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
            KillPlayer(REKTCanvas.TypeOfREKT.InActive);
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