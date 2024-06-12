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
    private int LevelSpawningCount = 0;
    private bool isBuildNavMeshOnLevelSpawnsComplete = false;    // when no levels spawing we can build nav mesh

    private void Awake()
    {
        Instance = this;
    }

    public void IncrementLevelSpawningCount()
    {
        LevelSpawningCount++;
    }

    public void DecrementLevelSpawningCount()
    {
        LevelSpawningCount--;
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
                networkObject.HasComponent<PlayerController>()) continue;

            // destroy everything else
            networkObject.Despawn();
        }

        // clear the list
        networkObjects.Clear();
    }

    private void CreateLevel(int index = 0)
    {
        // increment the spawning level counter and start watching this number so we can build nav mesh once it goes
        // back to zero
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

        if (isBuildNavMeshOnLevelSpawnsComplete && LevelSpawningCount <= 0)
        {
            isBuildNavMeshOnLevelSpawnsComplete = false;
            NavigationSurfaceSingleton.Instance.Surface.BuildNavMesh();
        }
    }

    public void GoToNextLevel()
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

        // for each player, try get new spawn point
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players) { player.TryGetNewSpawnPoint(); }
    }

    public bool IsLevelCreated()
    {
        return m_currentLevel.GetComponent<NetworkLevel>() != null;
    }

    public Vector3 GetPlayerSpawnPoint()
    {
        var networkLevel = m_currentLevel.GetComponent<NetworkLevel>();
        if (networkLevel == null)
        {
            Debug.Log("Error: Current level has no NetworkLevel component");
            return Vector3.zero;
        }

        return m_currentLevel.GetComponent<NetworkLevel>().PopPlayerSpawnPoint();
    }
}
