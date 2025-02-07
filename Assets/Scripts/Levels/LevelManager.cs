using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Cysharp.Threading.Tasks;
using Level;

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
    //private float m_depthCounter_SERVER = 0;

    // variables to keep track of spawning levels
    [HideInInspector] public int LevelSpawningCount = 0;
    //private bool isHandleLevelLoaded = false;    // when no levels spawing we can call level loaded code

    // variable for player spawning
    private List<Vector3> m_playerSpawnPositions = new List<Vector3>();
    private int m_spawnIndex = 0;

    // this helps other classes know where we are in a level transition
    //public enum TransitionState { Null, Start, ClientHeadsUp, GoToNext, ClientHeadsDown, End }
    //[HideInInspector] public NetworkVariable<TransitionState> transitionState;

    private float k_clientHeadsUpDuration_s = 0.5f;
    private float k_levelLoadingDuration_s = 1f;

    // this is for doing first spawn of level manager
    //private bool m_isSpawnedFirstLevel = false;

    //private bool m_isOnceOnlySpawnDone = false;

    private NetworkLevel.LevelType m_oldLevelType = NetworkLevel.LevelType.Null;
    private NetworkLevel.LevelType m_newLevelType = NetworkLevel.LevelType.Null;

    public event Action OnLevelChangeHeadsUp;
    public event Action<NetworkLevel.LevelType, NetworkLevel.LevelType> OnLevelChanged;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //transitionState = new NetworkVariable<TransitionState>(TransitionState.Null);
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // reset server vars
        if (IsServer)
        {
            //m_isSpawnedFirstLevel = false;
            //m_isOnceOnlySpawnDone = false;
            m_currentLevelIndex_SERVER = -1;
            //m_depthCounter_SERVER = 0;
            //transitionState.Value = TransitionState.Null;

            GoToDegenapeVillageLevel_SERVER();
        }

        // if new client, check if we've done the tutorial
        if (IsClient)
        {

            var isTutorialComplete = PlayerPrefs.GetInt("IsTutorialComplete", 0);
            if (isTutorialComplete == 0 && Bootstrap.Instance.ShowTutorialLevel)
            {
                Debug.Log("You need to do the tutorial!");
                //Debug.Log("Try spawn Tutorial level");
                //TryGoToTutorialLevelServerRpc();
            }
            else
            {
                //Debug.Log("Try spawn Degenape Village level");
                //TryGoToDegenapeVillageLevelServerRpc();
            }
        }
    }

    /*
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
    */

    /*
    public void GoToTutorialLevel_SERVER()
    {
        if (!IsServer) return;

        // reset level list and go to next level
        SetLevelList_SERVER(TutorialLevels);
        TransitionToNextLevel_SERVER();
        //m_depthCounter_SERVER = -TutorialLevels.Count;
    }
    */

    public void GoToDegenapeVillageLevel_SERVER()
    {
        if (!IsServer) return;

        // set ape village as level
        var levels = new List<GameObject>();
        levels.Add(ApeVillageLevel);

        // reset level list and go to next level
        SetLevelList_SERVER(levels);
        _ = TransitionToNextLevel_SERVER();
        //m_depthCounter_SERVER = 0;
    }

    public void SetLevelList_SERVER(List<GameObject> levels)
    {
        if (!IsServer) return;

        m_levels.Clear();
        m_levels = levels;
        m_currentLevelIndex_SERVER = -1;
    }

    public async UniTask TransitionToNextLevel_SERVER()
    {
        if (!IsServer) return;  // SERVER ONLY CODE BELOW

        // tell all clients to start transition
        OnLevelChangeHeadsUp?.Invoke();
        if (!IsHost) OnLevelChangeHeadsUp_ClientRpc();

        // wait a moment to give clients chance to start fade out screens
        await UniTask.Delay((int)(k_clientHeadsUpDuration_s * 1000));

        // perform leveltransition
        GoToNextLevel_SERVER();

        // wait till all level loaded correctly
        await HandleLevelLoaded_SERVER();

        // add further buffer to allow player new spawn position to propogate to client
        await UniTask.Delay((int)(k_levelLoadingDuration_s * 1000));

        // tell clients level has changed
        OnLevelChanged?.Invoke(m_oldLevelType, m_newLevelType);
        if (!IsHost) OnLevelChanged_ClientRpc(m_oldLevelType, m_newLevelType);
    }

    private void GoToNextLevel_SERVER()
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
        }

        // increment current level index
        m_currentLevelIndex_SERVER++;

        // create next level
        CreateLevel_SERVER(m_currentLevelIndex_SERVER);

        /// ALL BELOW CODE NEEDS TO BE MOVED BACK TO THEIR OWN CLASSES

        // increment all LevelCountBuffs
        var levelCountBuffs = FindObjectsByType<LevelCountedBuff>(FindObjectsSortMode.None);

        // do degenape/tutorial vs dungeon logic
        if (IsDegenapeVillage() || IsTutorial())
        {
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
            // increment all buffs
            foreach (var lcb in levelCountBuffs)
            {
                if (lcb != null) lcb.IncrementLevelCount();
            }
        }

        /// ABOVE CODE NEEDS TO BE MOVED BACK TO OWN CLASSES
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

        // check for a valid level
        if (m_levels[index] == null)
        {
            Debug.LogError("No valid level in LevelManager m_levels at index: " + index);
        }

        // spawn the level and save the new level index
        m_currentLevel = Instantiate(m_levels[index]);
        m_currentLevel.GetComponent<NetworkObject>().Spawn();
        m_currentLevelIndex_SERVER = index;
        m_oldLevelType = m_newLevelType;
        m_newLevelType = m_currentLevel.GetComponent<Level.NetworkLevel>().levelType;
        m_currentLevelType.Value = m_newLevelType;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnLevelChangeHeadsUp_ClientRpc()
    {
        Debug.Log("OnLevelChangeHeadsUp_ClientRpc()");
        OnLevelChangeHeadsUp?.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void OnLevelChanged_ClientRpc(NetworkLevel.LevelType oldLevelType, NetworkLevel.LevelType newLevelType)
    {
        Debug.Log("OnLevelChanged_ClientRpc()");
        OnLevelChanged?.Invoke(oldLevelType, newLevelType);
    }

    // update nav mesh, spawn things, drop spawn players
    async UniTask HandleLevelLoaded_SERVER()
    {
        if (!IsServer) return;

        while (LevelSpawningCount > 0)
        {
            await UniTask.Yield();
        }

        await UniTask.Yield();

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

        // update list of all available spawn positions
        m_playerSpawnPositions.Clear();
        var playerSpawnPoints = m_currentLevel.GetComponentsInChildren<PlayerSpawnPoints>();

        foreach (var psp in playerSpawnPoints) m_playerSpawnPositions.Add(psp.transform.position);
        if (m_playerSpawnPositions.Count == 0)
        {
            Debug.LogWarning("No PlayerSpawnPoint objects identified at first level of level prefab heirarchy, setting to Vector3.zero");
            m_playerSpawnPositions.Add(Vector3.zero);
        }

        // we cycle through the available positions for our spawn for each player
        m_spawnIndex = UnityEngine.Random.Range(0, m_playerSpawnPositions.Count);

        // NOTE: the below does not work for the very first spawn so we need to ensure that
        // the connection approval handler calls GetPlayerSpawnPosition() and sets positions too
        foreach (var pc in Game.Instance.playerControllers)
        {
            var playerPrediction = pc.GetComponent<PlayerPrediction>();
            if (playerPrediction != null)
            {
                playerPrediction.SetPlayerPosition(GetPlayerSpawnPosition());
            }
        }
    }

    public List<PlayerController> spawnedPlayers = new List<PlayerController>();

    public Vector3 GetPlayerSpawnPosition()
    {
        if (m_playerSpawnPositions.Count <= 0)
        {
            Debug.LogWarning("A level does not contain any PlayerSpawnPoints, returning Vector3.zero as spawn position");
            return Vector3.zero;
        }

        if (m_spawnIndex >= m_playerSpawnPositions.Count) m_spawnIndex = 0;

        var spawnPosition = m_playerSpawnPositions[m_spawnIndex];

        m_spawnIndex++;

        return spawnPosition;
    }


    void NumberAndNameLevel_SERVER()
    {
        if (!IsServer) return;

        m_numberAndLevelTimer -= Time.deltaTime;
        if (m_numberAndLevelTimer > 0) return;
        m_numberAndLevelTimer = k_numberAndLevelInterval;

        if (m_levels == null || m_levels.Count <= 0) return;
        if (m_currentLevelIndex_SERVER < 0) return;

        string number = "0";
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