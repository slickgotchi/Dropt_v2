using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class CleaveSwing : PlayerAbility
{
    [Header("Cleave Swing Parameters")]
    [SerializeField] float Projection = 1f;

    private Animator m_animator;
    public float attackRange = 2.0f;
    public int damage = 10;
    public float hitFeedbackDuration = 0.1f; // Duration of the enemy flashing white

    private int m_enemyLayer;

    private Collider2D m_collider;


    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("CleaveSwingSpawned " + IsServer);
        m_animator = GetComponent<Animator>();
        m_enemyLayer = 1 << LayerMask.NameToLayer("EnemyHurt");
        m_collider = GetComponent<Collider2D>();
    }
    /*
    public override void OnStart()
    {
        if (IsServer) Debug.Log("OnStart server");
        if (IsClient) Debug.Log("OnStart client");

        // align with calling players direction
        RotateToPlayerDirection();

        // offset from player slightly
        transform.localPosition = PlayerDirection * Projection;

        // Play the attack animation
        if (m_animator != null) m_animator.Play("CleaveSwing");

        // do a collision check
        m_collider.enabled = true;
        List<Collider2D> hitColliders = new List<Collider2D>();
        m_collider.Overlap(GetEnemyHurtContactFilter(), hitColliders);
        foreach (var hit in hitColliders)
        {
            if (IsServer)
            {
                Debug.Log("Hit on server");
                hit.GetComponent<NetworkCharacter>().HpCurrent.Value -= 20;
            }
            if (IsClient)
            {
                Debug.Log("Hit on client");
                if (hit.HasComponent<SpriteFlash>())
                {
                    hit.GetComponent<SpriteFlash>().DamageFlash();
                }
            }

        }

        hitColliders.Clear();
    }

    public override void OnFinish()
    {
    }


    [ServerRpc]
    private void PerformCleaveSwingServerRpc()
    {
        // Get enemies within attack range
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, m_enemyLayer);

        // Apply damage to each enemy
        foreach (Collider enemy in hitEnemies)
        {
            //enemy.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
    */
}
