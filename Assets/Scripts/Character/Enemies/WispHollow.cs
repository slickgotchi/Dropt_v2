using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WispHollow : MonoBehaviour
{
    public GameObject FudWispPrefab;
    public int MaxWisps = 3;
    public float WispSpawnInterval = 3f;
    public Vector3 SpawnOffset = new Vector3(0, 1.5f, 0);

    private float m_spawnTimer = 0f;
    private List<GameObject> m_liveWisps = new List<GameObject>();

    private void Awake()
    {
        m_spawnTimer = WispSpawnInterval;
    }

    private void Update()
    {
        if (Bootstrap.IsClient())
        {
            return;
        }

        m_spawnTimer -= Time.deltaTime;

        for (int i = m_liveWisps.Count - 1; i >= 0; i--)
        {
            if (m_liveWisps[i].gameObject == null)
            {
                m_liveWisps.RemoveAt(i);
            }
        }

        if (m_spawnTimer <= 0 && m_liveWisps.Count < MaxWisps)
        {
            var wisp = SpawnWisp();
            m_liveWisps.Add(wisp);
            m_spawnTimer += WispSpawnInterval;
        }
    }

    private GameObject SpawnWisp()
    {
        GameObject wisp = Instantiate(FudWispPrefab);
        wisp.transform.position = transform.position + SpawnOffset;
        wisp.GetComponent<NetworkObject>().Spawn();
        return wisp;
    }
}
