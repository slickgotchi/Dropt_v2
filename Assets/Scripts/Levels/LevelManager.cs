using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance { get; private set; }

    // level tracking variables
    [SerializeField] private List<GameObject> m_levels = new List<GameObject>();
    private GameObject m_currentLevel;
    private int m_currentLevelIndex = 0;

    // variables to keep track of spawning levels
    [HideInInspector] public int LevelSpawningCount = 0;
    private bool isBuildNavMeshOnLevelSpawnsComplete = false;    // when no levels spawing we can build nav mesh

    // variable for player spawning
    private List<Vector3> m_playerSpawnPoints = new List<Vector3>();

    public NetworkVariable<TransitionState> State;

    private void Awake()
    {
        Instance = this;
        State = new NetworkVariable<TransitionState>(TransitionState.Null);
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

        // spawn first level
        CreateLevel(0);
    }

    private void DestroyCurrentLevel()
    {
        var networkObjects = new List<NetworkObject>(FindObjectsByType<NetworkObject>(FindObjectsSortMode.None));

        // deparent every single network object
        foreach (var networkObject in networkObjects) { networkObject.transform.parent = null; }

        // destroy all network objects (except for some)
        foreach (var networkObject in networkObjects)
        {
            // things to save for next level
            if (networkObject.HasComponent<LevelManager>() ||
                networkObject.HasComponent<PlayerController>() ||
                networkObject.HasComponent<PlayerAbility>()) continue;

            // destroy everything else (remove onDestroy components first too to prevent the new objects appearing
            // in the next level)
            if (networkObject.HasComponent<OnDestroySpawnNetworkObject>())
            {
                networkObject.GetComponent<OnDestroySpawnNetworkObject>().SpawnPrefab = null;
            }
            networkObject.Despawn();
        }

        // clear the list
        networkObjects.Clear();
    }

    private void CreateLevel(int index = 0)
    {
        // increment the spawning level counter and start watching this number so we can
        // build nav mesh once it goes back to zero
        LevelSpawningCount++;
        isBuildNavMeshOnLevelSpawnsComplete = true;

        // spawn the level
        m_currentLevel = Instantiate(m_levels[index]);
        m_currentLevel.GetComponent<NetworkObject>().Spawn();
        m_currentLevelIndex = index;
    }

    private void Update()
    {
        if (!IsServer) return;

        HandleBuildNavMesh();

        HandleState();
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
            nextLevelIndex = 0;
        }

        // create next level and update current level index
        CreateLevel(nextLevelIndex);
        m_currentLevelIndex = nextLevelIndex;

        // drop spawn players
        var no_playerSpawnPoints = m_currentLevel.GetComponentInChildren<PlayerSpawnPoints>();
        m_playerSpawnPoints.Clear();
        for (int i = 0; i < no_playerSpawnPoints.transform.childCount; i++)
        {
            m_playerSpawnPoints.Add(no_playerSpawnPoints.transform.GetChild(i).transform.position);
        }

        // set each player spawn position
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var randIndex = UnityEngine.Random.Range(0, m_playerSpawnPoints.Count);
            var spawnPoint = m_playerSpawnPoints[randIndex];
            m_playerSpawnPoints.RemoveAt(randIndex);

            player.GetComponent<PlayerPrediction>().SetPlayerPosition(spawnPoint);
            player.GetComponent<PlayerGotchi>().DropSpawn(player.transform.position, spawnPoint);
        }
    }

    bool isBuildNextFrame = false;
    bool isNavMeshBuilt = false;

    void HandleBuildNavMesh()
    {
        if (!IsServer) return;

        if (isBuildNextFrame && !isNavMeshBuilt)
        {
            NavigationSurfaceSingleton.Instance.Surface.RemoveData();
            NavigationSurfaceSingleton.Instance.Surface.BuildNavMesh();
            isNavMeshBuilt = true;
        }

        // this code ensures we only build a navmesh once level is finished loading
        if (isBuildNavMeshOnLevelSpawnsComplete && LevelSpawningCount <= 0)
        {
            isBuildNavMeshOnLevelSpawnsComplete = false;
            isBuildNextFrame = true;
            isNavMeshBuilt = false;
        }
    }
}
