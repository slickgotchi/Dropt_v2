using UnityEngine;
using Unity.Netcode;

public class CleaveSwing : NetworkBehaviour
{
    public Animator animator;
    public float attackRange = 2.0f;
    public int damage = 10;
    public float hitFeedbackDuration = 0.1f; // Duration of the enemy flashing white

    private int m_enemyLayer;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        m_enemyLayer = 1 << LayerMask.NameToLayer("EnemyHurt");
    }

    public void PerformCleaveSwing()
    {
        // Play the attack animation
        animator.Play("CleaveSwing");

        // Perform immediate client-side feedback
        ClientFeedback();

        // Call the ServerRpc to perform damage calculations on the server
        PerformCleaveSwingServerRpc();
    }

    private void ClientFeedback()
    {
        // Get enemies within attack range
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, m_enemyLayer);

        // Trigger visual feedback on clients
        foreach (Collider enemy in hitEnemies)
        {
            //enemy.GetComponent<Enemy>().FlashWhite(hitFeedbackDuration);
        }
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

    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to show the attack range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
