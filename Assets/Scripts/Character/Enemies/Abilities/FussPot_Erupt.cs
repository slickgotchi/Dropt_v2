using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class FussPot_Erupt : EnemyAbility
{
    [Header("FussPot Erupt Parameters")]
    //public GameObject FussPot_EruptProjectilePrefab;
    public float LobProjectileDuraton = 1f;
    //[HideInInspector] public float ProjectileSpreadInDegrees = 30;
    private float MaxDistance = 8f;

    public List<FussPot_EruptProjectile> Projectiles = new List<FussPot_EruptProjectile>();

    public override void OnActivate()
    {
        if (!IsServer) return;
        if (Parent == null) { Debug.Log("Null ref detected"); return; }

        var parentNetworkCharacter = Parent.GetComponent<NetworkCharacter>();
        if (parentNetworkCharacter == null) { Debug.LogWarning("Null ref detected"); return; }

        var spawnPosition = Parent.transform.position + new Vector3(0, 1.5f, 0);

        //var dir = AttackDirection;
        //var distance = (PositionToAttack - spawnPosition).magnitude;
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

        distA = math.min(distA, MaxDistance);
        distB = math.min(distB, MaxDistance);
        distC = math.min(distC, MaxDistance);

        CreateAndFireProjectile(0, dirA, spawnPosition, distA, damage, criticalChance, criticalDamage);
        CreateAndFireProjectileClientRpc(0, dirA, spawnPosition, distA, damage, criticalChance, criticalDamage);

        CreateAndFireProjectile(1, dirB, spawnPosition, distB, damage, criticalChance, criticalDamage);
        CreateAndFireProjectileClientRpc(1, dirB, spawnPosition, distB, damage, criticalChance, criticalDamage);

        CreateAndFireProjectile(2, dirC, spawnPosition, distC, damage, criticalChance, criticalDamage);
        CreateAndFireProjectileClientRpc(2, dirC, spawnPosition, distC, damage, criticalChance, criticalDamage);
    }

    // Function to create and fire a projectile
    void CreateAndFireProjectile(int index, Vector3 direction, Vector3 spawnPosition,
        float distance, float damage, float criticalChance, float criticalDamage)
    {
        var projectile = Projectiles[index];
        if (projectile == null) { Debug.LogWarning("Null ref detected"); return; }

        var eruptProjectile = projectile.GetComponent<FussPot_EruptProjectile>();
        if (eruptProjectile == null) { Debug.LogWarning("Null ref detected"); return; }

        eruptProjectile.Init(
            spawnPosition,
            quaternion.identity,
            direction,
            distance,
            LobProjectileDuraton,
            damage,
            criticalChance,
            criticalDamage);

        eruptProjectile.Fire();
    }

    [Rpc(SendTo.NotServer)]
    void CreateAndFireProjectileClientRpc(int index, Vector3 direction, Vector3 spawnPosition,
        float distance, float damage, float criticalChance, float criticalDamage)
    {
        CreateAndFireProjectile(index, direction, spawnPosition,
            distance, damage, criticalChance, criticalDamage);
    }
}
