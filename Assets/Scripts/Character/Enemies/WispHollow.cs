using System.Collections.Generic;
using Dropt;
using Unity.Netcode;
using UnityEngine;

public class WispHollow : NetworkBehaviour
{
    public GameObject FudWispPrefab;
    public int MaxWisps = 3;
    public float WispSpawnInterval = 3f;
    public Transform SpawnPoint;

    private float m_spawnTimer = 0f;
    private List<GameObject> m_liveWisps = new List<GameObject>();
    private Animator m_animator;

    private List<GameObject> m_instancedWisps = new List<GameObject>();

    private SoundFX_WispHollow m_soundFX_WispHollow;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            return;
        }

        m_animator = GetComponent<Animator>();
        m_soundFX_WispHollow = GetComponent<SoundFX_WispHollow>();
        m_animator.Play("WispHollow_Spawn");
        m_spawnTimer = WispSpawnInterval;
        m_soundFX_WispHollow.PlaySpawnSound();
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
        wisp.transform.position = SpawnPoint != null ? SpawnPoint.transform.position : transform.position;
        wisp.SetActive(false);

        // DO NOT SPAWN DIRECTLY AFTER INSTANTIATING, FOR SOME REASON UNITY NEEDS A FRAME TO GO BY FOR NAVMESH TO WORK CORRECTLY BEFORE SPAWNING
        // USE THE DEFERRED SPAWNER
        DeferredSpawner.SpawnNextFrame(wisp.GetComponent<NetworkObject>());

        return wisp;
    }

    private void OnFudWispDespawn(GameObject fudWisp)
    {
        m_liveWisps.Remove(fudWisp);
    }

    //public override void OnNetworkDespawn()
    //{
    //base.OnNetworkDespawn();
    //if (m_soundFX_WispHollow != null) m_soundFX_WispHollow.PlayDieSound();
    //}
}