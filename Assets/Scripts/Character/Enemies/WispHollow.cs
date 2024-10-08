using System.Collections.Generic;
using Dropt;
using Unity.Netcode;
using UnityEngine;

public class WispHollow : NetworkBehaviour
{
    public GameObject FudWispPrefab;
    public int MaxWisps = 3;
    public float WispSpawnInterval = 3f;
    public Vector3 SpawnOffset = new Vector3(0, 1.5f, 0);

    private float m_spawnTimer = 0f;
    private List<GameObject> m_liveWisps = new List<GameObject>();
    private Animator m_animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            return;
        }

        m_animator = GetComponent<Animator>();
        m_animator.Play("WispHollow_Spawn");
        m_spawnTimer = WispSpawnInterval;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        m_spawnTimer -= Time.deltaTime;

        if (m_spawnTimer <= 0 && m_liveWisps.Count < MaxWisps)
        {
            GameObject wisp = SpawnWisp();
            m_liveWisps.Add(wisp);
            m_spawnTimer += WispSpawnInterval;
        }
    }

    private GameObject SpawnWisp()
    {
        m_animator.Play("WispHollow_SpawnWisp");
        GameObject wisp = Instantiate(FudWispPrefab);
        EnemyAI_FudWisp enemyAI_FudWisp = wisp.GetComponent<EnemyAI_FudWisp>();
        enemyAI_FudWisp.AssignDespawnAction(OnFudWispDespawn);
        wisp.transform.position = transform.position + SpawnOffset;
        wisp.GetComponent<NetworkObject>().Spawn();
        return wisp;
    }

    private void OnFudWispDespawn(GameObject fudWisp)
    {
        m_liveWisps.Remove(fudWisp);
    }
}