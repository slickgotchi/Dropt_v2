using UnityEngine;
using Unity.Netcode;

public class AttackCentre : NetworkBehaviour
{
    public void SpawnAttackEffect(Vector3 attackerPosition)
    {
        Vector3 enemyPosition = transform.position;
        Vector3 direction = (attackerPosition - enemyPosition).normalized;
        Vector3 attackEffectPosition = enemyPosition + (direction * 0.3f);
        SpawnEffectClientRpc(attackEffectPosition);
    }

    [ClientRpc]
    private void SpawnEffectClientRpc(Vector3 position)
    {
        VisualEffectsManager.Singleton.SpawnPetAttackEffect(position);
    }
}
