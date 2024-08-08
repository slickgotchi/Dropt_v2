using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpiderController : NetworkBehaviour
{
    private float m_spawnTimer = 0;
    public bool m_isSpawnFinished = false;

    public override void OnNetworkSpawn()
    {
        m_spawnTimer = GetComponent<EnemyController>().SpawnDuration;

        if (m_spawnTimer > 0)
        {
            GetComponent<Animator>().Play("Spider_Jump");
        }
    }

    private void Update()
    {
        if (!IsSpawned) return;

        m_spawnTimer -= Time.deltaTime;

        if (m_spawnTimer <= 0 && !m_isSpawnFinished)
        {
            m_isSpawnFinished = true;
            GetComponent<Animator>().Play("Spider_Walk");
        }
    }
}