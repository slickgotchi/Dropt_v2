using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class CleaveSwing : PlayerAbility
{
    public Animator animator;
    public float attackRange = 2.0f;
    public int damage = 10;
    public float hitFeedbackDuration = 0.1f; // Duration of the enemy flashing white

    private int m_enemyLayer;

    private Collider2D m_collider;


    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        m_enemyLayer = 1 << LayerMask.NameToLayer("EnemyHurt");
        m_collider = GetComponent<Collider2D>();
        m_collider.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
    }

    public void PerformCleaveSwing(Vector3 pos)
    {
        gameObject.SetActive(true);

        transform.position = pos;

        // Play the attack animation
        animator.Play("CleaveSwing");

        // enable collisions
        m_collider.enabled = true;

        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << LayerMask.NameToLayer("EnemyHurt"),
            useTriggers = true,
        };

        List<Collider2D> hitColliders = new List<Collider2D>();
        int numHits = m_collider.Overlap(contactFilter, hitColliders);

        foreach (var hit in hitColliders)
        {
            if (hit.HasComponent<SpriteFlash>())
            {
                hit.GetComponent<SpriteFlash>().DamageFlash();
                Debug.Log("we got a hit!");
            }
        }

        hitColliders.Clear();
    }

    public void FinishCleaveSwing()
    {
        m_collider.enabled = false;
        gameObject.SetActive(false);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.GetComponent<SpriteFlash>() != null)
    //    {
    //        collision.GetComponent<SpriteFlash>().DamageFlash();
    //    }
    //}

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
}
