using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StunExplosion : NetworkBehaviour
{
    private float m_timer = 1f;
    private ParticleSystem m_particleSystem;

    private void Awake()
    {
        m_particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    public override void OnNetworkSpawn()
    {
        if (m_particleSystem != null)
        {
            // Get the main module and modify the simulation speed
            var mainModule = m_particleSystem.main;
            mainModule.simulationSpeed = 3f;
        }
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer <= 0f && IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
