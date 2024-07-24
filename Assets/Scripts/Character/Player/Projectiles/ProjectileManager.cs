using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileManager : NetworkBehaviour
{
    //public static ProjectileManager Instance { get; private set; }

    //public GameObject cleaveCycloneProjectilePrefab;
    //public GameObject ballisticShotProjectile_ArrowPrefab;
    //public GameObject ballisticShotProjectile_BasketballPrefab;
    //public GameObject ballisticShotProjectile_BulletPrefab;
    //public GameObject magicCastProjectile_OrbPrefab;
    //public GameObject splashLobProjectilePrefab;
    //public GameObject throwProjectilePrefab;

    //private List<GameObject> cleaveCycloneProjectilePool = new List<GameObject>();
    //private List<GameObject> ballisticShotProjectile_ArrowPool = new List<GameObject>();
    //private List<GameObject> ballisticShotProjectile_BasketballPool = new List<GameObject>();
    //private List<GameObject> ballisticShotProjectile_BulletPool = new List<GameObject>();
    //private List<GameObject> magicCastProjectile_OrbPool = new List<GameObject>();
    //private List<GameObject> splashLobProjectilePool = new List<GameObject>();
    //private List<GameObject> throwProjectilePool = new List<GameObject>();

    //private void Awake()
    //{
    //    Instance = this;
    //}

    //public override void OnNetworkSpawn()
    //{
    //    if (IsServer)
    //    {
    //        InitializePool(cleaveCycloneProjectilePrefab, cleaveCycloneProjectilePool, 5);
    //        InitializePool(ballisticShotProjectile_ArrowPrefab, ballisticShotProjectile_ArrowPool, 5);
    //        InitializePool(ballisticShotProjectile_BasketballPrefab, ballisticShotProjectile_BasketballPool, 5);
    //        InitializePool(ballisticShotProjectile_BulletPrefab, ballisticShotProjectile_BulletPool, 5);
    //        InitializePool(magicCastProjectile_OrbPrefab, magicCastProjectile_OrbPool, 5);
    //        InitializePool(splashLobProjectilePrefab, splashLobProjectilePool, 20);
    //        InitializePool(throwProjectilePrefab, throwProjectilePool, 5);
    //    }
    //}

    //private void InitializePool(GameObject prefab, List<GameObject> pool, int poolSize)
    //{
    //    for (int i = 0; i < poolSize; i++)
    //    {
    //        GameObject instance = Instantiate(prefab);
    //        instance.SetActive(false);
    //        instance.GetComponent<NetworkObject>().Spawn();
    //        pool.Add(instance);
    //    }
    //}

    //public GameObject GetProjectileFromPool(List<GameObject> pool)
    //{
    //    foreach (var projectile in pool)
    //    {
    //        if (!projectile.activeInHierarchy)
    //        {
    //            projectile.SetActive(true);
    //            return projectile;
    //        }
    //    }
    //    Debug.LogWarning("No available projectiles in the pool.");
    //    return null; // Handle this case appropriately in your game logic
    //}

    //public void ReturnProjectileToPool(GameObject projectile)
    //{
    //    if (!IsServer) return;

    //    projectile.SetActive(false);
    //}

    //// Convenience methods to get specific types of projectiles
    //public GameObject GetCleaveCycloneProjectile() => GetProjectileFromPool(cleaveCycloneProjectilePool);
    //public GameObject GetBallisticShotProjectile_Arrow() => GetProjectileFromPool(ballisticShotProjectile_ArrowPool);
    //public GameObject GetBallisticShotProjectile_Basketball() => GetProjectileFromPool(ballisticShotProjectile_BasketballPool);
    //public GameObject GetBallisticShotProjectile_Bullet() => GetProjectileFromPool(ballisticShotProjectile_BulletPool);
    //public GameObject GetMagicCastProjectile_Orb() => GetProjectileFromPool(magicCastProjectile_OrbPool);
    //public GameObject GetSplashLobProjectile() => GetProjectileFromPool(splashLobProjectilePool);
    //public GameObject GetThrowProjectile() => GetProjectileFromPool(throwProjectilePool);
}
