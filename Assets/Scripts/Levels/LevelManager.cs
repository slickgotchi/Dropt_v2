using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance { get; private set; }

    // level tracking variables
    public GameObject ApeVillageLevel;
    private List<GameObject> m_levels = new List<GameObject>();

    private GameObject m_currentLevel;
    [HideInInspector] public int m_currentLevelIndex = -1;

    // variables to keep track of spawning levels
    [HideInInspector] public int LevelSpawningCount = 0;
    private bool isHandleLevelLoaded = false;    // when no levels spawing we can call level loaded code

    // variable for player spawning
    private List<Vector3> m_playerSpawnPoints = new List<Vector3>();

    public NetworkVariable<TransitionState> State;
    public NetworkVariable<int> CurrentLevelIndex = new NetworkVariable<int>(0);

    private List<NetworkObject> m_networkObjectSpawns = new List<NetworkObject>();

    private float m_depthCounter = 0;

    public Level.NetworkLevel GetCurrentNetworkLevel()
    {
        if (m_currentLevel == null) return null;
        return m_currentLevel.GetComponent<Level.NetworkLevel>();
    }

    public void AddToSpawnList(NetworkObject networkObject)
    {
        m_networkObjectSpawns.Add(networkObject);
    }

    public void SetLevelList(List<GameObject> levels)
    {
        m_levels.Clear();
        m_levels = levels;
        m_currentLevelIndex = -1;
    }

    private void Awake()
    {
        Instance = this;
        State = new NetworkVariable<TransitionState>(TransitionState.Null);

        if (IsServer)
        {
            CurrentLevelIndex.Value = m_currentLevelIndex;
        }
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        GoToDegenapeVillageLevel();
    }

    public void GoToDegenapeVillageLevel()
    {
        // set ape village as level
        var levels = new List<GameObject>();
        levels.Add(ApeVillageLevel);
        SetLevelList(levels);

        // proceed to our new next level
        GoToNextLevel();

        // set depth counter to 0
        m_depthCounter = 0;


    }

    public bool IsDegenapeVillage()
    {
        if (m_levels == null) return false;
        if (m_levels.Count <= 0) return false;

        return (m_levels[CurrentLevelIndex.Value] == ApeVillageLevel);
    }

    public void DestroyCurrentLevel()
    {
        if (!IsServer) return;

        // disable proximity manager
        ProximityManager.Instance.enabled = false;

        // tag all spawns to die
        LevelSpawnManager.Instance.TagAllCurrentLevelSpawnsForDead();

        // find everything to destroy
        var destroyObjects = new List<DestroyAtLevelChange>(FindObjectsByType<DestroyAtLevelChange>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        // remove any parents
        foreach (var destroyObject in destroyObjects)
        {
            destroyObject.transform.parent = null;
        }

        // activate objects and add the ignore proximity component
        foreach (var destroyObject in destroyObjects)
        {
            destroyObject.gameObject.SetActive(true);
            destroyObject.gameObject.AddComponent<IgnoreProximity>();
        }

        // despawn/destroy all objects
        foreach (var destroyObject in destroyObjects)
        {
            // disable any onDestroySpawn something components
            if (destroyObject.HasComponent<OnDestroySpawnNetworkObject>())
            {
                destroyObject.GetComponent<OnDestroySpawnNetworkObject>().enabled = false;
            }
            if (destroyObject.HasComponent<OnDestroySpawnGltr>())
            {
                destroyObject.GetComponent<OnDestroySpawnGltr>().enabled = false;
            }
            if (destroyObject.HasComponent<Interactables.Chest>())
            {
                destroyObject.GetComponent<Interactables.Chest>().enabled = false;
            }

            // destroy object
            if (destroyObject != null && destroyObject.HasComponent<NetworkObject>() && IsServer)
            {
                if (destroyObject.GetComponent<NetworkObject>().IsSpawned)
                {
                    destroyObject.GetComponent<NetworkObject>().Despawn();
                }
            }
            else
            {
                Destroy(destroyObject);
            }
        }

        // clear our list
        destroyObjects.Clear();


        // re-enable proximity manager
        ProximityManager.Instance.enabled = true;
    }

    private void CreateLevel(int index = 0)
    {
        // increment the spawning level counter and start watching this number so we can
        // build nav mesh once it goes back to zero
        LevelSpawningCount++;
        isHandleLevelLoaded = true;

        // check for a valid level
        if (m_levels[index] == null)
        {
            Debug.LogError("No valid level in LevelManager m_levels at index: " + index);
        }

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

            NumberAndNameLevel();
        }
    }

    private float k_numberAndLevelInterval = 0.5f;
    private float m_numberAndLevelTimer = 0f;

    void NumberAndNameLevel()
    {
        if (!IsServer) return;

        m_numberAndLevelTimer -= Time.deltaTime;
        if (m_numberAndLevelTimer > 0) return;
        m_numberAndLevelTimer = k_numberAndLevelInterval;

        if (m_levels == null || m_levels.Count <= 0) return;
        if (m_currentLevelIndex < 0 || CurrentLevelIndex.Value < 0) return;

        string number = m_depthCounter.ToString();
        string name = m_levels[CurrentLevelIndex.Value].name;

        NumberAndNameLevelClientRpc(number, name);
    }

    [ClientRpc]
    void NumberAndNameLevelClientRpc(string number, string name)
    {
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

        // if we are on last level, return to degenape
        if (m_currentLevelIndex >= m_levels.Count - 1)
        {
            var levels = new List<GameObject>();
            levels.Add(ApeVillageLevel);
            SetLevelList(levels);
        }

        // destroy current level
        DestroyCurrentLevel();

        // get index for next level
        int nextLevelIndex = m_currentLevelIndex + 1;

        // create next level and update current level index
        CreateLevel(nextLevelIndex);
        m_currentLevelIndex = nextLevelIndex;

        // increment all LevelCountBuffs
        var levelCountBuffs = FindObjectsByType<LevelCountedBuff>(FindObjectsSortMode.None);
        foreach (var lcb in levelCountBuffs)
        {
            lcb.IncrementLevelCount();
        }

        // increase depth counter
        m_depthCounter++;
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
            //NavigationSurfaceSingleton.Instance.Surface.UpdateNavMesh(NavigationSurfaceSingleton.Instance.Surface.navMeshData);
            //NavigationSurfaceSingleton.Instance.Surface.BuildNavMesh();

            var navMeshes = FindObjectsByType<NavMeshPlus.Components.NavMeshSurface>(FindObjectsSortMode.None);
            foreach (var surface in navMeshes)
            {
                surface.BuildNavMesh();
            }

            // Spawn everything in the spawn list
            for (int i = 0; i < m_networkObjectSpawns.Count; i++)
            {
                var no = m_networkObjectSpawns[i];

                no.Spawn();
            }
            m_networkObjectSpawns.Clear();

            // clear out old spawn points
            if (IsHost)
            {
                var playerSpawns = new List<PlayerSpawnPoints>(m_currentLevel.GetComponentsInChildren<PlayerSpawnPoints>());
                foreach (var playerSpawn in playerSpawns)
                {
                    Object.Destroy(playerSpawn.gameObject);
                }
            }

            m_playerSpawnPoints.Clear();

            // drop spawn players
            var no_playerSpawnPoints = m_currentLevel.GetComponentsInChildren<PlayerSpawnPoints>();
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
                    }
                }
            }

            for (int i = 0; i < makeupCount; i++)
            {
                // if we got at least one legit spawn use that
                if (m_playerSpawnPoints.Count > 0)
                {
                    m_playerSpawnPoints.Add(m_playerSpawnPoints[0]);
                }
                else
                {
                    m_playerSpawnPoints.Add(Vector3.zero);
                }
            }

            // get all players to recheck their spawn position
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                player.IsLevelSpawnPositionSet = false;
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

    public bool IsPlayerSpawnPointsReady()
    {
        return m_playerSpawnPoints.Count > 0;
    }

    public Vector3? TryGetPlayerSpawnPoint()
    {
        if (m_playerSpawnPoints.Count <= 0) return null;

        var randIndex = UnityEngine.Random.Range(0, m_playerSpawnPoints.Count);
        var spawnPoint = m_playerSpawnPoints[randIndex];
        m_playerSpawnPoints.RemoveAt(randIndex);

        return spawnPoint;
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
    }
}