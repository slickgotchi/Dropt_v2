using System.Collections;
using System.Collections.Generic;
using Audio.Game;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.AI;
using CarlosLab.UtilityIntelligence;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance { get; private set; }

    // level tracking variables
    [SerializeField] private List<GameObject> m_levels = new List<GameObject>();
    public int TutorialStartLevel = 0;
    public int DegenapeVillageLevel = 2;
    public int DungeonStartLevel = 3;

    private GameObject m_currentLevel;
    [HideInInspector] public int m_currentLevelIndex = 0;

    // variables to keep track of spawning levels
    [HideInInspector] public int LevelSpawningCount = 0;
    private bool isHandleLevelLoaded = false;    // when no levels spawing we can call level loaded code

    // variable for player spawning
    private List<Vector3> m_playerSpawnPoints = new List<Vector3>();

    public NetworkVariable<TransitionState> State;
    public NetworkVariable<int> CurrentLevelIndex = new NetworkVariable<int>(0);

    private List<NetworkObject> m_networkObjectSpawns = new List<NetworkObject>();

    public void AddToSpawnList(NetworkObject networkObject)
    {
        m_networkObjectSpawns.Add(networkObject);
    }


    private void Awake()
    {
        Instance = this;
        State = new NetworkVariable<TransitionState>(TransitionState.Null);
    }

    private void OnDestroy()
    {
        if (null != GameAudioManager.Instance)
        {
            GameAudioManager.Instance.PLAY_SOUND -= OnPlaySound;
        }
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (m_levels.Count <= 0)
        {
            Debug.Log("Error: No prefab levels added to the LevelManager!");
            return;
        }

        if (Game.Instance.IsTutorialCompleted())
        {
            GoToDegenapeVillageLevel();
        }
        else
        {
            GoToTutorialLevel();
        }


        GameAudioManager.Instance.PLAY_SOUND += OnPlaySound;
    }

    public void GoToTutorialLevel()
    {
        CreateLevel(0);
    }

    public void GoToDegenapeVillageLevel()
    {
        m_currentLevelIndex = DegenapeVillageLevel - 1;
        GoToNextLevel();
    }

    private void DestroyCurrentLevel()
    {
        var networkObjects = new List<NetworkObject>(FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        // deparent every single network object
        foreach (var networkObject in networkObjects) 
        {
            // things to save for next level
            if (networkObject.HasComponent<LevelManager>() ||
                networkObject.HasComponent<PlayerController>() ||
                networkObject.HasComponent<PlayerAbility>() ||
                networkObject.HasComponent<DontDestroyAtLevelChange>()) continue;

            networkObject.TryRemoveParent(); 
        }

        // destroy all network objects (except for some)
        foreach (var networkObject in networkObjects)
        {
            // things to save for next level
            if (networkObject.HasComponent<LevelManager>() ||
                networkObject.HasComponent<PlayerController>() ||
                networkObject.HasComponent<PlayerAbility>() ||
                networkObject.HasComponent<DontDestroyAtLevelChange>()) continue;

            // remove onDestroy components first too to prevent the new objects appearing
            if (networkObject.HasComponent<OnDestroySpawnNetworkObject>())
            {
                networkObject.GetComponent<OnDestroySpawnNetworkObject>().enabled = false;
            }
            if (networkObject.HasComponent<OnDestroySpawnGltr>())
            {
                networkObject.GetComponent<OnDestroySpawnGltr>().enabled = false;
            }

            // Ensure the object is active before despawning
            if (!networkObject.gameObject.activeInHierarchy)
            {
                networkObject.gameObject.SetActive(true);
            }

            // despawn
            if (IsServer)
            {
                if (networkObject.HasComponent<UtilityAgentFacade>())
                {
                    networkObject.GetComponent<UtilityAgentFacade>().Destroy();
                } else
                {
                    if (networkObject.IsSpawned)
                    {
                        networkObject.Despawn();
                    }
                }
            }

        }

        // clear the list
        networkObjects.Clear();
    }

    private void CreateLevel(int index = 0)
    {
        // increment the spawning level counter and start watching this number so we can
        // build nav mesh once it goes back to zero
        LevelSpawningCount++;
        isHandleLevelLoaded = true;

        // spawn the level
        m_currentLevel = Instantiate(m_levels[index]);
        m_currentLevel.GetComponent<NetworkObject>().Spawn();
        m_currentLevelIndex = index;
    }

    private void Update()
    {
        if (IsServer)
        {
            HandleLevelLoaded();

            HandleState();

            CurrentLevelIndex.Value = m_currentLevelIndex;
        }

        if (IsClient)
        {
            NumberAndNameLevel();
        }

    }

    private float k_numberAndLevelInterval = 0.5f;
    private float m_numberAndLevelTimer = 0f;

    void NumberAndNameLevel()
    {
        m_numberAndLevelTimer -= Time.deltaTime;
        if (m_numberAndLevelTimer > 0) return;
        m_numberAndLevelTimer = k_numberAndLevelInterval;

        string number = CurrentLevelIndex.Value <= DegenapeVillageLevel ? "-" : (CurrentLevelIndex.Value - DegenapeVillageLevel).ToString();
        string name = m_levels[CurrentLevelIndex.Value].name;
        PlayerHUDCanvas.Singleton.SetLevelNumberAndName(number, Dropt.Utils.String.ConvertToReadableString(name));
    }

    // 1. Receive GoToNextLevel message from other part of server
    // 2. Set StartLevelTransition message/state on all clients to prepare for level transition
    // 3. Wait small duration (typically 300ms) to allow for fade transitions
    // 4. Destroy old level, create new level, move players to new spawn positions
    // 5. Set EndLevelTransition message/state on all clients so they can fade out
    void HandleState()
    {
        switch (State.Value)
        {
            case TransitionState.Start:
                m_headsUpTimer = m_headsUpDuration;
                State.Value = TransitionState.ClientHeadsUp;
                break;
            case TransitionState.ClientHeadsUp:
                m_headsUpTimer -= Time.deltaTime;
                if (m_headsUpTimer <= 0)
                {
                    State.Value = TransitionState.GoToNext;
                }
                break;
            case TransitionState.GoToNext:
                HandleGoToNext();
                m_headsDownTimer = m_headsDownDuration;
                State.Value = TransitionState.ClientHeadsDown;
                break;
            case TransitionState.ClientHeadsDown:
                m_headsDownTimer -= Time.deltaTime;
                if (m_headsDownTimer <= 0)
                {
                    State.Value = TransitionState.End;
                }
                break;
            case TransitionState.End:

                State.Value = TransitionState.Null;
                break;
            default:
                break;
        }
    }

    public enum TransitionState
    {
        Null, Start, ClientHeadsUp, GoToNext, ClientHeadsDown, End,
    }

    private float m_headsUpDuration = 1f;
    private float m_headsUpTimer = 0;

    private float m_headsDownDuration = 1f;
    private float m_headsDownTimer = 0;

    public void GoToNextLevel()
    {
        if (!IsServer) return;

        State.Value = TransitionState.Start;
    }

    private void HandleGoToNext()
    {
        if (!IsServer) return;

        // destroy current level
        DestroyCurrentLevel();

        // get index for next level
        int nextLevelIndex = m_currentLevelIndex + 1;
        if (nextLevelIndex >= m_levels.Count)
        {
            // return to the degenape village
            nextLevelIndex = DegenapeVillageLevel;
        }

        // create next level and update current level index
        CreateLevel(nextLevelIndex);
        m_currentLevelIndex = nextLevelIndex;

        // if our new level is degenape, tell client they can mark tutorial as complete
        if (m_currentLevelIndex == DegenapeVillageLevel)
        {
            MarkTutorialCompletedClientRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void MarkTutorialCompletedClientRpc()
    {
        Game.Instance.CompleteTutorial();
    }

    bool isHandleLevelLoadedNextFrame = false;
    bool isLevelLoaded = false;

    // update nav mesh, spawn things, drop spawn players
    void HandleLevelLoaded()
    {
        if (!IsServer) return;

        if (isHandleLevelLoadedNextFrame && !isLevelLoaded)
        {
            isLevelLoaded = true;

            // Update nav mesh
            NavigationSurfaceSingleton.Instance.Surface.UpdateNavMesh(NavigationSurfaceSingleton.Instance.Surface.navMeshData);

            // Spawn everything in the spawn list
            for (int i = 0; i < m_networkObjectSpawns.Count; i++)
            {
                var no = m_networkObjectSpawns[i];

                no.Spawn();
            }
            m_networkObjectSpawns.Clear();

            // drop spawn players
            var no_playerSpawnPoints = m_currentLevel.GetComponentsInChildren<PlayerSpawnPoints>();
            m_playerSpawnPoints.Clear();
            int makeupCount = 3;
            if (no_playerSpawnPoints != null)
            {
                for (int i = 0; i < no_playerSpawnPoints.Length; i++)
                {
                    var playerSpawnPoints = no_playerSpawnPoints[i];
                    for (int j = 0; j < playerSpawnPoints.transform.childCount; j++)
                    {
                        m_playerSpawnPoints.Add(playerSpawnPoints.transform.GetChild(j).transform.position);
                        makeupCount--;
                        Debug.Log("add legit spawn point: " + playerSpawnPoints.transform.GetChild(j).transform.position);
                    }
                }
            }

            for (int i = 0; i < makeupCount; i++)
            {
                // if we got at least one legit spawn use that
                if (m_playerSpawnPoints.Count > 0)
                {
                    m_playerSpawnPoints.Add(m_playerSpawnPoints[0]);
                } else
                {
                    m_playerSpawnPoints.Add(Vector3.zero);
                }

            }

            // set each player spawn position
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                var randIndex = UnityEngine.Random.Range(0, m_playerSpawnPoints.Count);
                var spawnPoint = m_playerSpawnPoints[randIndex];
                m_playerSpawnPoints.RemoveAt(randIndex);

                player.GetComponent<PlayerPrediction>().SetPlayerPosition(spawnPoint);
                player.GetComponent<PlayerGotchi>().DropSpawn(spawnPoint);
            }

            // destroy all spawn points
            m_playerSpawnPoints.Clear();

            // if is host we should delete spawn points
            if (IsHost)
            {
                var playerSpawns = new List<PlayerSpawnPoints>(m_currentLevel.GetComponentsInChildren<PlayerSpawnPoints>());
                foreach (var playerSpawn in playerSpawns)
                {
                    Object.Destroy(playerSpawn.gameObject);
                }
            }
        }

        // this code ensures we only build a navmesh once level is finished loading
        if (isHandleLevelLoaded && LevelSpawningCount <= 0)
        {
            isHandleLevelLoaded = false;
            isHandleLevelLoadedNextFrame = true;
            isLevelLoaded = false;
        }
    }


    private void OnPlaySound(string type, Vector3 position, ulong id)
    {
        if (!IsServer || id != 0)
            return;

        PlaySoundClientRpc(type, position, id);
    }

    [Rpc(SendTo.NotMe)]
    void PlaySoundClientRpc(string type, Vector3 position, ulong id)
    {
        GameAudioManager.Instance.PlaySoundForMe(type, position);
    }
}