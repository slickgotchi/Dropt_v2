using UnityEngine;

public class EnemyDetactor : MonoBehaviour
{
    [SerializeField] private Vector2 boxSize = new(20, 16);
    [SerializeField] private LayerMask enemyLayer;

    public Collider2D[] DetectEnemies(Transform player)
    {
        // Center of the box (relative to the GameObject's position)
        Vector2 boxCenter = player.position;

        // Perform the OverlapBox check (no rotation, so Quaternion.identity for 2D)
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, enemyLayer);

        return hitEnemies;
    }
}