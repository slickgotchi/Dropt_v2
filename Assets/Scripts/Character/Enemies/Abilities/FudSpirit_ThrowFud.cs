using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FudSpirit_ThrowFud : EnemyAbility
{
    [Header("FudSpirit_ThrowFud Parameter")]
    public GameObject FudProjectile;
    public float Distance = 5f;
    public float Duration = 2f;


    public override void OnActivate()
    {
        if (Parent == null) { Debug.LogWarning("Null ref detected"); return; }

        var parentNetworkCharacter = Parent.GetComponent<NetworkCharacter>();
        if (parentNetworkCharacter == null) { Debug.LogWarning("Null ref detected"); return; }

        var parentEnemyAI = Parent.GetComponent<Dropt.EnemyAI>();
        if (parentEnemyAI == null) { Debug.LogWarning("Null ref detected"); return; }

        // get direction and parent centre position
        var attackDir = parentEnemyAI.AttackDirection;
        var attackCentrePos = Dropt.Utils.Battle.GetAttackCentrePosition(Parent);

        // init and spawn projectile
        var projectile = Instantiate(FudProjectile);
        projectile.GetComponent<GenericEnemyProjectile>().Init(
            attackCentrePos,
            PlayerAbility.GetRotationFromDirection(attackDir),
            attackDir,
            Distance,
            Duration,
            parentNetworkCharacter.currentStaticStats.AttackPower,
            parentNetworkCharacter.currentStaticStats.CriticalChance,
            parentNetworkCharacter.currentStaticStats.CriticalDamage,
            Parent,
            1);

        var projectileNetworkObject = projectile.GetComponent<NetworkObject>();
        if (projectileNetworkObject == null) { Debug.LogWarning("Null ref detected"); return; }

        var projectileGenericEnemyProjectile = projectile.GetComponent<GenericEnemyProjectile>();
        if (projectileGenericEnemyProjectile == null) { Debug.LogWarning("Null ref detected"); return; }

        projectileNetworkObject.Spawn();
        projectileGenericEnemyProjectile.Fire();

        var parentEnemyController = Parent.GetComponent<EnemyController>();
        if (parentEnemyController == null) { Debug.LogWarning("Null ref detected"); return; }

        // orient the parent fud spirit sprite
        EnemyController.Facing facing = AttackDirection.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        parentEnemyController.SetFacing(facing, 1f);
    }
}
