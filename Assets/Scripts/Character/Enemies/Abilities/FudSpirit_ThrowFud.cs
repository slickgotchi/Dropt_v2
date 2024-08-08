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


    public override void OnExecutionStart()
    {
        // get direction
        var dir = (Target.GetComponent<AttackCentre>().transform.position - Parent.GetComponent<AttackCentre>().transform.position).normalized;

        // init and spawn projectile
        var projectile = Instantiate(FudProjectile);
        projectile.GetComponent<GenericEnemyProjectile>().Init(
            Parent.transform.position + new Vector3(0, 0.3f, 0),
            PlayerAbility.GetRotationFromDirection(dir),
            dir,
            Distance,
            Duration,
            Parent.GetComponent<NetworkCharacter>().AttackPower.Value,
            Parent.GetComponent<NetworkCharacter>().CriticalChance.Value,
            Parent.GetComponent<NetworkCharacter>().CriticalDamage.Value,
            Parent,
            1);
        projectile.GetComponent<NetworkObject>().Spawn();
        projectile.GetComponent<GenericEnemyProjectile>().Fire();

        // orient the parent fud spirit sprite
        EnemyController.Facing facing = dir.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        Parent.GetComponent<EnemyController>().SetFacingDirection(facing, 1f);
    }
}
