using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns a dash trail object at the spawners location
public class DashTrailSpawner : MonoBehaviour
{
    [SerializeField] private DashTrail dashTrail;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private float delay = 0.2f;

    private bool m_isStarted = false;
    private float m_timer;
    private float m_spawnTimer = 0f;
    private float m_delayTimer = 0f;

    public void StartSpawning()
    {
        m_isStarted = true;
        m_timer = duration;
        m_delayTimer = delay;
    }

    private void Update()
    {
        if (!m_isStarted) return;

        m_delayTimer -= Time.deltaTime;
        if (m_delayTimer > 0f) return;

        m_timer -= Time.deltaTime;
        m_spawnTimer += Time.deltaTime;

        if (m_spawnTimer > spawnInterval)
        {
            var goDashTrail = Instantiate(dashTrail);
            goDashTrail.transform.position = transform.position;
            m_spawnTimer -= spawnInterval;
        }

        if (m_timer < 0f)
        {
            m_isStarted = false;
        }
    }
}
