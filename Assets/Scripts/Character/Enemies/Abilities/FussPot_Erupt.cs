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
        if (Parent == null) { Debug.Log("Null ref detected"); return; }

        var parentNetworkCharacter = Parent.GetComponent<NetworkCharacter>();
        if (parentNetworkCharacter == null) { Debug.LogWarning("Null ref detected"); return; }

        var spawnPosition = Parent.transform.position + new Vector3(0, 1.5f, 0);

        var dir = AttackDirection;
        var distance = (PositionToAttack - spawnPosition).magnitude;
        var damage = parentNetworkCharacter.currentStaticStats.AttackPower;
        var criticalChance = parentNetworkCharacter.currentStaticStats.CriticalChance;
        var criticalDamage = parentNetworkCharacter.currentStaticStats.CriticalDamage;


        // create three random points around the player
        var radiansA = UnityEngine.Random.Range(0f, 360 * math.PI / 180);
        var radiansB = radiansA + 1f / 3f * 2 * math.PI;
        var radiansC = radiansA - 1f / 3f * 2 * math.PI;

        Vector3 offsetA = new Vector3(math.cos(radiansA), math.sin(radiansA), 0);
        Vector3 offsetB = new Vector3(math.cos(radiansB), math.sin(radiansB), 0);
        Vector3 offsetC = new Vector3(math.cos(radiansC), math.sin(radiansC), 0);

        float offsetDistA = UnityEngine.Random.Range(1f, 3f);
        float offsetDistB = UnityEngine.Random.Range(1f, 3f);
        float offsetDistC = UnityEngine.Random.Range(1f, 3f);

        var landPosA = PositionToAttack + offsetA * offsetDistA;
        var landPosB = PositionToAttack + offsetB * offsetDistB;
        var landPosC = PositionToAttack + offsetC * offsetDistC;

        var dirA = (landPosA - spawnPosition).normalized;
        var dirB = (landPosB - spawnPosition).normalized;
        var dirC = (landPosC - spawnPosition).normalized;

        var distA = (landPosA - spawnPosition).magnitude;
        var distB = (landPosB - spawnPosition).magnitude;
        var distC = (landPosC - spawnPosition).magnitude;

        CreateAndFireProjectile(dirA, spawnPosition, distA, damage, criticalChance, criticalDamage);

        CreateAndFireProjectile(dirB, spawnPosition, distB, damage, criticalChance, criticalDamage);

        CreateAndFireProjectile(dirC, spawnPosition, distC, damage, criticalChance, criticalDamage);

        /*
        // Fire the first projectile with a little distance randomness
        var randA = UnityEngine.Random.Range(-1f, 1f);
        CreateAndFireProjectile(dir, spawnPosition, distance + randA, damage, criticalChance, criticalDamage);

        // Calculate and fire the projectile at 120 degrees
        var rotation120 = Quaternion.Euler(0, 0, ProjectileSpreadInDegrees);
        var dir120 = rotation120 * dir;
        CreateAndFireProjectile(dir120, spawnPosition, distance, damage, criticalChance, criticalDamage);

        // Calculate and fire the projectile at -120 degrees
        var rotationMinus120 = Quaternion.Euler(0, 0, -ProjectileSpreadInDegrees);
        var dirMinus120 = rotationMinus120 * dir;
        CreateAndFireProjectile(dirMinus120, spawnPosition, distance, damage, criticalChance, criticalDamage);
        */
    }

    // Function to create and fire a projectile
    void CreateAndFireProjectile(Vector3 direction, Vector3 spawnPosition,
        float distance, float damage, float criticalChance, float criticalDamage)
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

        var projectileNetworkObject = projectile.GetComponent<NetworkObject>();
        if (projectileNetworkObject == null) { Debug.LogWarning("Null ref detected"); return; }

        var projectileEruptProjectile = projectile.GetComponent<FussPot_EruptProjectile>();
        if (projectileEruptProjectile == null) { Debug.LogWarning("Null ref detected"); return; }

        projectileNetworkObject.Spawn();
        projectileEruptProjectile.Fire();
    }
}
