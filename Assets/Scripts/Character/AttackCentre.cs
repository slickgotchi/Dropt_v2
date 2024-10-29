using UnityEngine;
using Unity.Netcode;

public class AttackCentre : NetworkBehaviour
{
    public void SpawnAttackEffect()
    {
        SpawnEffectClientRpc();
    }

    [ClientRpc]
    private void SpawnEffectClientRpc()
    {
        Vector3 attackEffectPosition = GetRandomPosition(transform.position, 0.3f);
        GameObject effect = VisualEffectsManager.Singleton.SpawnPetAttackEffect(attackEffectPosition);
        effect.transform.SetParent(transform);
    }

    public Vector3 GetRandomPosition(Vector3 center, float radius)
    {
        // Get a random point in a unit circle and scale it by the radius
        Vector2 randomPoint = Random.insideUnitCircle * radius;

        // Offset the point by the center position
        Vector3 randomPosition = new Vector3(center.x + randomPoint.x, center.y, center.z + randomPoint.y);

        return randomPosition;
    }
}
