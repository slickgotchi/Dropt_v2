using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class LevelManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> m_levels = new List<GameObject>();

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (m_levels.Count <= 0)
        {
            Debug.Log("Add a level to the LevelManager!");
        }

        // spawn first level
        Debug.Log("Spawn first level/lobby");
        var spawnLevel = Instantiate(m_levels[0]);
        spawnLevel.GetComponent<NetworkObject>().Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
