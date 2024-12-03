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

    public static float InactiveTimerDuration = 5 * 60;

    public bool IsDead = false;

    public bool IsInvulnerable { get; private set; }

    private float m_inactiveTimer = InactiveTimerDuration;

    private HoldBarCanvas m_holdBarCanvas;

    public bool IsLevelSpawnPositionSet = false;

    private AttackCentre m_playerAttackCentre;

    // tracking selected gotchi
    private int m_selectedGotchiId = 0;

    // variables for tracking current gotchi
    private int m_localGotchiId = 0;
    public NetworkVariable<int> NetworkGotchiId = new NetworkVariable<int>(69420);

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

        // local player
        if (IsLocalPlayer)
        {
            m_cameraFollower = GameObject.FindGameObjectWithTag("CameraFollower");
            m_cameraFollower.GetComponent<CameraFollowerAndPlayerInteractor>().Player = gameObject;

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

            //SetNetworkGotchiIdServerRpc(gotchiId);
        }
        else
        {
            ScreenBlockers.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    [Rpc(SendTo.Server)]
    private void SetNetworkGotchiIdServerRpc(int gotchiId)
    {
        NetworkGotchiId.Value = gotchiId;
    }

    public void KillPlayer(REKTCanvas.TypeOfREKT typeOfREKT)
    {
        if (!IsServer) return;

        GetComponent<PlayerController>().IsDead = true;
        TriggerGameOverClientRpc(GetComponent<NetworkObject>().NetworkObjectId, typeOfREKT);
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
    private void TriggerGameOverClientRpc(ulong playerNetworkObjectId, REKTCanvas.TypeOfREKT typeOfREKT)
    {
        //ensure we only trigger this for the relevant player
        var player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        var localId = GetComponent<NetworkObject>().NetworkObjectId;
        if (player.NetworkObjectId != localId) return;

        // show the game over canvas
        REKTCanvas.Instance.Show(typeOfREKT);

        // shutdown the networkmanager for the client
        NetworkManager.Singleton.Shutdown();
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
                m_positionText.text = $"({pos.x:F2}, {pos.y:F2})";
            }

            HandleLevelTransition();

            // setup player hud
            if (!m_isPlayerHUDInitialized && GetComponent<NetworkCharacter>() != null)
            {
                PlayerHUDCanvas.Instance.SetLocalPlayerCharacter(GetComponent<PlayerCharacter>());
            }

            // Set camera to follow player
            if (m_cameraFollower != null)
            {
                m_cameraFollower.transform.position = m_playerPrediction.GetLocalPlayerInterpPosition() + new Vector3(0, 0.5f, 0f);
            }

            HandleNextLevelCheat();

            // check for player input to ensure we stay active
            CheckForPlayerInput();
        }

        // Handle level spawning on the server
        if (IsServer && !IsLevelSpawnPositionSet)
        {
            var pos = LevelManager.Instance.TryGetPlayerSpawnPoint();
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

    [Rpc(SendTo.ClientsAndHost)]
    private void SetCameraPositionClientRpc(Vector3 position, ulong networkObjectId)
    {
        if (GetComponent<NetworkObject>().NetworkObjectId != networkObjectId) return;

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

        // Restore original damping settings
        //framingTransposer.m_XDamping = originalDamping;
        //framingTransposer.m_YDamping = originalDamping;
        //framingTransposer.m_ZDamping = originalDamping;

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
    }

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
    private bool shouldBeBlackedOut = true;
    private bool isBlackedOut = true;
    private bool isFirstLoad = true;

    // handle loading canvas
    private void HandleLevelTransition()
    {
        if (!IsLocalPlayer) return;

        LevelManager.TransitionState state = LevelManager.Instance.transitionState.Value;

        if (state == LevelManager.TransitionState.Start ||
            state == LevelManager.TransitionState.ClientHeadsUp ||
            state == LevelManager.TransitionState.GoToNext)
        {
            shouldBeBlackedOut = true;

            // disable player input
            GetComponent<PlayerPrediction>().IsInputEnabled = false;
        }

        if (state == LevelManager.TransitionState.ClientHeadsDown ||
            state == LevelManager.TransitionState.End ||
            state == LevelManager.TransitionState.Null)
        {
            shouldBeBlackedOut = false;
        }

        if (shouldBeBlackedOut && isFirstLoad)
        {
            LoadingCanvas.Instance.Animator.Play("LoadingCanvas_Default");
        }

        if (shouldBeBlackedOut && !isBlackedOut && !isFirstLoad)
        {
            LoadingCanvas.Instance.Animator.Play("LoadingCanvas_WipeIn");
            isBlackedOut = true;
        }

        if (!shouldBeBlackedOut && isBlackedOut)
        {
            LoadingCanvas.Instance.Animator.Play("LoadingCanvas_WipeOut");
            REKTCanvas.Instance.Container.SetActive(false);
            isBlackedOut = false;
            isFirstLoad = false;
        }
    }

    // cheat to go to next level
    private void HandleNextLevelCheat()
    {
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    GoNextLevelServerRpc();
        //}
    }

    [Rpc(SendTo.Server)]
    private void GoNextLevelServerRpc()
    {
        LevelManager.Instance.StartTransitionToNextLevel_SERVER();
    }

    private void HandleDegenapeHpAp()
    {
        if (!IsServer) return;

        if (LevelManager.Instance.IsDegenapeVillage())
        {
            m_networkCharacter.HpCurrent.Value = m_networkCharacter.HpMax.Value;
            m_networkCharacter.ApCurrent.Value = m_networkCharacter.ApMax.Value;
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
}
