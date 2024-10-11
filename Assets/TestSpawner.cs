using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TestSpawner : MonoBehaviour
{
    public GameObject spawnPrefab;

    

    private void Start()
    {

    }

    private GameObject m_object;

    void Update()
    {
        if (m_object != null)
        {
            m_object.SetActive(true);
            m_object.GetComponent<NetworkObject>().Spawn();
            Debug.Log("spawned new object");
            m_object = null;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (m_object == null)
            {
                // now instantiate
                m_object = Object.Instantiate(spawnPrefab);
                m_object.transform.position = new Vector3(-12, 2, 0);
                m_object.SetActive(false);
                Debug.Log("instantiated new object");
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            var enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                enemy.GetComponent<NetworkCharacter>().TakeDamage(100000000, false, player[0].gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            var navMeshes = FindObjectsByType<NavMeshPlus.Components.NavMeshSurface>(FindObjectsSortMode.None);
            foreach (var surface in navMeshes)
            {
                surface.BuildNavMesh();
                Debug.Log("Rebuilt surface");
            }
        }
    }
}
