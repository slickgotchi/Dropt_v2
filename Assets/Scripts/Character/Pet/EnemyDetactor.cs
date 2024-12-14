using UnityEngine;

public class EnemyDetactor : MonoBehaviour
{
    [SerializeField] private Vector2 boxSize = new(20, 16);
    [SerializeField] private LayerMask enemyLayer;

    private Transform m_transform;

    private void Start()
    {
        m_transform = transform;
    }

    public Collider2D[] DetectEnemies()
    {
        // Center of the box (relative to the GameObject's position)
        Vector2 boxCenter = m_transform.position;

        // Perform the OverlapBox check (no rotation, so Quaternion.identity for 2D)
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, enemyLayer);

        return hitEnemies;
    }
}