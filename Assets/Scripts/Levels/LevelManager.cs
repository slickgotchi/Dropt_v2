using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Cysharp.Threading.Tasks;

// Level Management
// - Four types of level
// -- Tutorial
// -- DegenapeVillage
// -- Dungeon
// -- DungeonRest

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance { get; private set; }

    // network level prefabs
    public List<GameObject> TutorialLevels = new List<GameObject>();
    public GameObject ApeVillageLevel;

    // level tracking
    private List<GameObject> m_levels = new List<GameObject>();
    private GameObject m_currentLevel;
    private int m_currentLevelIndex_SERVER = -1;
    [HideInInspector] public NetworkVariable<Level.NetworkLevel.LevelType> m_currentLevelType =
        new NetworkVariable<Level.NetworkLevel.LevelType>(Level.NetworkLevel.LevelType.Null);

    // var for numbering/naming levels
    private float k_numberAndLevelInterval = 0.5f;
    private float m_numberAndLevelTimer = 0f;
    private float m_depthCounter_SERVER = 0;

    // variables to keep track of spawning levels
    [HideInInspector] public int LevelSpawningCount = 0;
    private bool isHandleLevelLoaded = false;    // when no levels spawing we can call level loaded code

    // variable for player spawning
    private List<Vector3> m_playerSpawnPoints = new List<Vector3>();

    // this helps other classes know where we are in a level transition
    public enum TransitionState { Null, Start, ClientHeadsUp, GoToNext, ClientHeadsDown, End }
    [HideInInspector] public NetworkVariable<TransitionState> transitionState;

    private float m_headsUpDuration = 0.5f;
    private float m_headsUpTimer = 0;

    private float m_headsDownDuration = 0.5f;
    private float m_headsDownTimer = 0;

    // this is for doing first spawn of level manager
    private bool m_isSpawnedFirstLevel = false;

    private bool m_isOnceOnlySpawnDone = false;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transitionState = new NetworkVariable<TransitionState>(TransitionState.Null);
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //Debug.Log("LevelManager spawned");

        // reset server vars
        if (IsServer)
        {
            m_isSpawnedFirstLevel = false;
            m_isOnceOnlySpawnDone = false;
            m_currentLevelIndex_SERVER = -1;
            m_depthCounter_SERVER = 0;
            transitionState.Value = TransitionState.Null;
        }

        // if new client, check if we've done the tutorial
        if (IsClient)
        {

            var isTutorialComplete = PlayerPrefs.GetInt("IsTutorialComplete", 0);
            if (isTutorialComplete == 0 && Bootstrap.Instance.ShowTutorialLevel)
            {
                //Debug.Log("Try spawn Tutorial level");
                TryGoToTutorialLevelServerRpc();
            }
            else
            {
                //Debug.Log("Try spawn Degenape Village level");
                TryGoToDegenapeVillageLevelServerRpc();
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void TryGoToTutorialLevelServerRpc()
    {
        // ensure this only works once
        if (!m_isOnceOnlySpawnDone)
        {
            GoToTutorialLevel_SERVER();
            m_isOnceOnlySpawnDone = true;
        }
    }

    [Rpc(SendTo.Server)]
    public void TryGoToDegenapeVillageLevelServerRpc()
    {
        // ensure this only works once
        if (!m_isOnceOnlySpawnDone)
        {
            GoToDegenapeVillageLevel_SERVER();
            m_isOnceOnlySpawnDone = true;
        }
    }

    public void GoToTutorialLevel_SERVER()
    {
        if (!IsServer) return;

        // reset level list and go to next level
        SetLevelList_SERVER(TutorialLevels);
        StartTransitionToNextLevel_SERVER();
        m_depthCounter_SERVER = -TutorialLevels.Count;
    }

    public void GoToDegenapeVillageLevel_SERVER()
    {
        if (!IsServer) return;

        // set ape village as level
        var levels = new List<GameObject>();
        levels.Add(ApeVillageLevel);

        // reset level list and go to next level
        SetLevelList_SERVER(levels);
        StartTransitionToNextLevel_SERVER();
        m_depthCounter_SERVER = 0;
    }

    public void SetLevelList_SERVER(List<GameObject> levels)
    {
        if (!IsServer) return;

        m_levels.Clear();
        m_levels = levels;
        m_currentLevelIndex_SERVER = -1;
    }

    public void DestroyCurrentLevel_SERVER()
    {
        if (!IsServer) return;

        // disable proximity manager
        ProximityManager.Instance.enabled = false;

        // find everything to destroy
        var destroyObjects = FindObjectsByType<DestroyAtLevelChange>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // remove any parents (NOTE: this should only apply to NetworkObjects! any embedded scene objects
        // with DestroyAtLevelChange should still be destroyed)
        foreach (var destroyObject in destroyObjects)
        {
            // its important everything is enabled (the proximity manager disables things)
            destroyObject.gameObject.SetActive(true);
            destroyObject.isDestroying = true;

            // deparent objects
            if (destroyObject.HasComponent<NetworkObject>())
            {
                destroyObject.transform.parent = null;
            }

            // if has levelspawn, remove it from the level spawn manager
            var levelSpawn = destroyObject.GetComponent<Level.LevelSpawn>();
            if (levelSpawn != null)
            {
                LevelSpawnManager.Instance.RemoveLevelSpawnComponent(levelSpawn);
            }
        }

        // despawn/destroy all objects
        foreach (var destroyObject in destroyObjects)
        {
            // get our destroy objects networkobject
            var networkObject = destroyObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                if (networkObject.IsSpawned)
                {
                    networkObject.Despawn();
                }
                else
                {
                    networkObject.Spawn();
                    networkObject.Despawn();
                }
            }
            else
            {
                Destroy(destroyObject);
            }
        }

        // re-enable proximity manager
        ProximityManager.Instance.enabled = true;
    }

    private void CreateLevel_SERVER(int index = 0)
    {
        if (!IsServer) return;

        // increment the spawning level counter and start watching this number so we can
        // build nav mesh once it goes back to zero
        LevelSpawningCount++;
        isHandleLevelLoaded = true;

        // check for a valid level
        if (m_levels[index] == null)
        {
            Debug.LogError("No valid level in LevelManager m_levels at index: " + index);
        }

        // spawn the level and save the new level index
        m_currentLevel = Instantiate(m_levels[index]);
        m_currentLevel.GetComponent<NetworkObject>().Spawn();
        m_currentLevelIndex_SERVER = index;
        m_currentLevelType.Value = m_currentLevel.GetComponent<Level.NetworkLevel>().levelType;
    }

    private void Update()
    {
        // handle server functions
        if (IsServer)
        {
            HandleLevelTransitions_SERVER();    // controls all transitions
            HandleLevelLoaded_SERVER();         // called once only when all levels/sublevels are loaded
            NumberAndNameLevel_SERVER();        // updates name/number of current level
        }
    }

    // 1. Receive GoToNextLevel message from other part of server
    // 2. Set StartLevelTransition message/state on all clients to prepare for level transition
    // 3. Wait small duration (typically 300ms) to allow for fade transitions
    // 4. Destroy old level, create new level, move players to new spawn positions
    // 5. Set EndLevelTransition message/state on all clients so they can fade out
    void HandleLevelTransitions_SERVER()
    {
        if (!IsServer) return;

        switch (transitionState.Value)
        {
            case TransitionState.Start:
                m_headsUpTimer = m_headsUpDuration;
                transitionState.Value = TransitionState.ClientHeadsUp;
                break;
            case TransitionState.ClientHeadsUp:
                m_headsUpTimer -= Time.deltaTime;
                if (m_headsUpTimer <= 0)
                {
                    transitionState.Value = TransitionState.GoToNext;
                }
                break;
            case TransitionState.GoToNext:
                HandleGoToNextLevel_SERVER();

                m_headsDownTimer = m_headsDownDuration;
                transitionState.Value = TransitionState.ClientHeadsDown;
                break;
            case TransitionState.ClientHeadsDown:
                m_headsDownTimer -= Time.deltaTime;
                if (m_headsDownTimer <= 0)
                {
                    transitionState.Value = TransitionState.End;
                }
                break;
            case TransitionState.End:
                //transitionState.Value = TransitionState.Null;
                break;
            default:
                break;
        }
    }

    public void StartTransitionToNextLevel_SERVER()
    {
        if (!IsServer) return;

        transitionState.Value = TransitionState.Start;
    }

    private void HandleGoToNextLevel_SERVER()
    {
        if (!IsServer) return;

        // destroy current level
        DestroyCurrentLevel_SERVER();

        // return to DegemApe village if we were on last level (should not occur in actual game), return to degenape
        if (m_currentLevelIndex_SERVER >= m_levels.Count - 1)
        {
            var levels = new List<GameObject>();
            levels.Add(ApeVillageLevel);
            SetLevelList_SERVER(levels);

            // WE DO EXIT DUNGEON CHECKS IN PLAYEROFFCHAINDATA (NOT HERE)
            //// do exist calcs for all players
            //var playerOffchainDatas = FindObjectsByType<PlayerOffchainData>(FindObjectsSortMode.None);
            //foreach (var pocd in playerOffchainDatas)
            //{
            //    pocd.ExitDungeonCalculateBalances(true);
            //}
        }

        // increment current level index
        m_currentLevelIndex_SERVER++;

        // create next level
        CreateLevel_SERVER(m_currentLevelIndex_SERVER);

        // increment all LevelCountBuffs
        var levelCountBuffs = FindObjectsByType<LevelCountedBuff>(FindObjectsSortMode.None);

        // do degenape/tutorial vs dungeon logic
        if (IsDegenapeVillage() || IsTutorial())
        {
            m_depthCounter_SERVER = 0;

            // set playres leaderboard loggers
            foreach (var pc in Game.Instance.playerControllers)
            {
                var pll = pc.GetComponent<PlayerLeaderboardLogger>();
                if (pll != null)
                {
                    pll.dungeonType = LeaderboardLogger.DungeonType.Adventure;
                }
            }

            // destroy all buffs
            foreach (var lcb in levelCountBuffs)
            {
                Destroy(lcb.gameObject);
            }
        }
        else
        {
            m_depthCounter_SERVER++;

            // increment all buffs
            foreach (var lcb in levelCountBuffs)
            {
                if (lcb != null) lcb.IncrementLevelCount();
            }
        }

        isLevelLoaded = false;
    }

    // vars for handling level loaded
    //bool isHandleLevelLoadedNextFrame = false;
    public bool isLevelLoaded = true;
    public bool isPlayersSpawnable = false;

    // update nav mesh, spawn things, drop spawn players
    async UniTaskVoid HandleLevelLoaded_SERVER()
    {
        if (!IsServer) return;

        //if (isHandleLevelLoadedNextFrame && !isLevelLoaded)
        if (!isLevelLoaded && LevelSpawningCount <= 0)
        {
            isLevelLoaded = false;
            await UniTask.Yield();
            isLevelLoaded = true;

            // check if level uses render mesh or physics colliders
            var networkLevel = m_levels[m_currentLevelIndex_SERVER].GetComponent<Level.NetworkLevel>();

            // rebuild all NavMeshSurfaces
            var navMeshes = FindObjectsByType<NavMeshPlus.Components.NavMeshSurface>(FindObjectsSortMode.None);
            foreach (var surface in navMeshes)
            {
                if (networkLevel.navmeshGeneration == Level.NetworkLevel.NavmeshGeneration.PhysicsColliders)
                {
                    surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
                }
                else
                {
                    surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.RenderMeshes;
                }

                surface.BuildNavMesh();
            }

            // clear out old spawn points
            if (IsHost)
            {
                var playerSpawns = new List<PlayerSpawnPoints>(m_currentLevel.GetComponentsInChildren<PlayerSpawnPoints>());
                foreach (var playerSpawn in playerSpawns)
                {
                    GameObject.Destroy(playerSpawn.gameObject);
                }
            }

            m_playerSpawnPoints.Clear();

            // drop spawn players
            var no_playerSpawnPoints = m_currentLevel.GetComponentsInChildren<PlayerSpawnPoints>();
            if (no_playerSpawnPoints != null)
            {
                for (int i = 0; i < no_playerSpawnPoints.Length; i++)
                {
                    var playerSpawnPoints = no_playerSpawnPoints[i];
                    for (int j = 0; j < playerSpawnPoints.transform.childCount; j++)
                    {
                        m_playerSpawnPoints.Add(playerSpawnPoints.transform.GetChild(j).transform.position);
                    }
                }
            }

            // add extra spawn points if we didn't get 3
            for (int i = 0; i < 3 - m_playerSpawnPoints.Count; i++)
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

            // clear the spawnedPlayers lists which will mean any player checking the list
            // will know if they need to spawn or not
            // NOTE: players need to check both this list and the state of isLevelLoaded
            spawnedPlayers.Clear();
            isPlayersSpawnable = true;

        }

        // this code ensures we only build a navmesh once level is finished loading
        //if (isHandleLevelLoaded && LevelSpawningCount <= 0)
        //{
        //    isHandleLevelLoaded = false;
        //    isHandleLevelLoadedNextFrame = true;
        //    isLevelLoaded = false;
        //}
    }

    public List<PlayerController> spawnedPlayers = new List<PlayerController>();

    public bool IsPlayerSpawnPointsReady()
    {
        return m_playerSpawnPoints.Count > 0;
    }

    public Vector3? TryGetPlayerSpawnPoint()
    {
        if (m_playerSpawnPoints.Count <= 0) return Vector3.zero;

        var randIndex = UnityEngine.Random.Range(0, m_playerSpawnPoints.Count);
        var spawnPoint = m_playerSpawnPoints[randIndex];

        if (!LevelManager.Instance.IsDegenapeVillage())
        {
            m_playerSpawnPoints.RemoveAt(randIndex);
        }

        return spawnPoint;
    }


    void NumberAndNameLevel_SERVER()
    {
        if (!IsServer) return;

        m_numberAndLevelTimer -= Time.deltaTime;
        if (m_numberAndLevelTimer > 0) return;
        m_numberAndLevelTimer = k_numberAndLevelInterval;

        if (m_levels == null || m_levels.Count <= 0) return;
        if (m_currentLevelIndex_SERVER < 0) return;

        string number = m_depthCounter_SERVER.ToString();
        string name = m_levels[m_currentLevelIndex_SERVER].name;
        string objective = m_levels[m_currentLevelIndex_SERVER].gameObject.GetComponent<Level.NetworkLevel>().objective;

        NumberAndNameLevelClientRpc(number, name, objective);
    }

    [ClientRpc]
    void NumberAndNameLevelClientRpc(string number, string name, string objective)
    {
        PlayerHUDCanvas.Instance.SetLevelNumberNameObjective(number, Dropt.Utils.String.ConvertToReadableString(name), objective);
    }

    // utility functions for determining where we are
    public Level.NetworkLevel.LevelType GetCurrentLevelType()
    {
        return m_currentLevelType.Value;
    }

    public bool IsTutorial()
    {
        return m_currentLevelType.Value == Level.NetworkLevel.LevelType.Tutorial;
    }

    public bool IsDegenapeVillage()
    {
        return m_currentLevelType.Value == Level.NetworkLevel.LevelType.DegenapeVillage;
    }

    public bool IsDungeon()
    {
        return m_currentLevelType.Value == Level.NetworkLevel.LevelType.Dungeon;
    }

    public bool IsDungeonRest()
    {
        return m_currentLevelType.Value == Level.NetworkLevel.LevelType.DungeonRest;
    }
}