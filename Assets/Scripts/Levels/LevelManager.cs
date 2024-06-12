using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private List<GameObject> m_levels = new List<GameObject>();

    private GameObject m_currentLevel;

    private List<NetworkObject> m_currentLevelObjects = new List<NetworkObject>();

    private void Awake()
    {
        Instance = this;
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
        m_currentLevel = Instantiate(m_levels[0]);
        m_currentLevel.GetComponent<NetworkObject>().Spawn();
    }

    public void GoToNextLevel()
    {

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

    public void RegisterObjectWithCurrentLevel(NetworkObject networkObject)
    {
        m_currentLevelObjects.Add(networkObject);
    }
}
