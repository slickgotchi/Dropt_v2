using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SnailSlash : NetworkBehaviour
{
    private float m_timer = 1f;

    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        m_timer = 1f;

        if (m_animator != null)
        {
            m_animator.Play("SnailSlash_Attack");

        }
        else
        {
            Debug.Log("No animator");
        }
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (!IsServer) return;

        m_timer -= Time.deltaTime;

        if (m_timer <= 0)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
