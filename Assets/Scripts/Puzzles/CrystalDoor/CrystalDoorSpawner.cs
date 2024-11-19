using System.Collections.Generic;
using UnityEngine;

public class CrystalDoorSpawner : MonoBehaviour
{
    [HideInInspector] public int spawnerId = -1;

    [SerializeField] private List<GameObject> m_doorsPrefab;

    public GameObject GetRandomDoorPrefab()
    {
        return m_doorsPrefab[Random.Range(0, m_doorsPrefab.Count)];
    }
}