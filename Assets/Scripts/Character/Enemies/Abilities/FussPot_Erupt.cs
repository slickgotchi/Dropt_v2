using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class FussPot_Erupt : EnemyAbility
{
    [Header("FussPot Erupt Parameters")]
    public GameObject FussPot_EruptProjectilePrefab;
    public float LobProjectileDuraton = 1f;
    [HideInInspector] public float ProjectileSpreadInDegrees = 30;

    public override void OnActivate()
    {
        var spawnPosition = Parent.transform.position + new Vector3(0, 1.5f, 0);

        var dir = AttackDirection;
        var distance = (PositionToAttack - Parent.transform.position).magnitude;
        var damage = Parent.GetComponent<NetworkCharacter>().AttackPower.Value;
        var criticalChance = Parent.GetComponent<NetworkCharacter>().CriticalChance.Value;
        var criticalDamage = Parent.GetComponent<NetworkCharacter>().CriticalDamage.Value;

        // Function to create and fire a projectile
        void CreateAndFireProjectile(Vector3 direction)
        {
            var projectile = Instantiate(FussPot_EruptProjectilePrefab);
            projectile.GetComponent<FussPot_EruptProjectile>().Init(
                spawnPosition,
                quaternion.identity,
                direction,
                PlayerAbility.GetRandomVariation(distance),
                LobProjectileDuraton,
                damage,
                criticalChance,
                criticalDamage);
            projectile.GetComponent<NetworkObject>().Spawn();
            projectile.GetComponent<FussPot_EruptProjectile>().Fire();
        }

        // Fire the first projectile
        CreateAndFireProjectile(dir);

        // Calculate and fire the projectile at 120 degrees
        var rotation120 = Quaternion.Euler(0, 0, ProjectileSpreadInDegrees);
        var dir120 = rotation120 * dir;
        CreateAndFireProjectile(dir120);

        // Calculate and fire the projectile at -120 degrees
        var rotationMinus120 = Quaternion.Euler(0, 0, -ProjectileSpreadInDegrees);
        var dirMinus120 = rotationMinus120 * dir;
        CreateAndFireProjectile(dirMinus120);
    }
}
