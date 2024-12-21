using System.Collections.Generic;
using Core.Pool;
using PickupItems.Orb;
using Unity.Netcode;
using UnityEngine;

public sealed class PickupItemManager : NetworkBehaviour
{
    public static PickupItemManager Instance { get; private set; }

    private Dictionary<GameObject, GameObject> m_prefabByInstanceMap;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        m_prefabByInstanceMap = new Dictionary<GameObject, GameObject>();
    }

    public enum Size
    {
        Tiny,
        Small,
        Medium,
        Large
    }

    public GameObject GltrOrbPrefab;
    public GameObject CGHSTOrbPrefab;
    public GameObject HpCannister;
    public GameObject EssenceCannister;
    public GameObject HpOrbPrefab;
    public GameObject ApOrbPrefab;

    public void SpawnGltr(int value, Vector3 position)
    {
        if (!IsServer) return;

        while (value > 0)
        {
            if (value >= 100)
            {
                GenerateGltrOrb(Size.Large, position);
                value -= 100;
            }
            else if (value >= 25)
            {
                GenerateGltrOrb(Size.Medium, position);
                value -= 25;
            }
            else if (value >= 5)
            {
                GenerateGltrOrb(Size.Small, position);
                value -= 5;
            }
            else
            {
                GenerateGltrOrb(Size.Tiny, position);
                value -= 1;
            }
        }
    }

    public void SpawnBigCGHST(Vector3 position)
    {
        if (!IsServer) return;
        GenerateCGHSTOrb(Size.Large, position);
    }

    public void SpawnSmallCGHST(Vector3 position)
    {
        if (!IsServer) return;
        GenerateCGHSTOrb(Size.Small, position);
    }

    public void SpawnApOrb(Size size, Vector3 position)
    {
        if (!IsServer) return;
        GenerateOrb<ApOrb>(ApOrbPrefab, size, position);
    }

    public void SpawnHpOrb(Size size, Vector3 position)
    {
        if (!IsServer) return;
        GenerateOrb<HpOrb>(HpOrbPrefab, size, position);
    }

    public void SpawnHpCannister(Vector3 position)
    {
        if (!IsServer) return;
        GenerateCannister(HpCannister, position);
    }

    public void SpawnEssenceCannister(Vector3 position)
    {
        if (!IsServer) return;
        GenerateCannister(EssenceCannister, position);
    }

    private void GenerateGltrOrb(Size size, Vector3 position, float rand = 0.3f)
    {
        GenerateOrb<GltrOrb>(GltrOrbPrefab, size, position, rand);
    }

    private TOrb GenerateOrb<TOrb>(GameObject prefab, Size size, Vector3 position, float rand = 0.3f)
        where TOrb : BaseOrb
    {
        var deltaX = Random.Range(-rand, rand);
        var deltaY = Random.Range(-rand, rand);
        var randPosition = position + new Vector3(deltaX, deltaY, 0);

        var networkObj = GetFromPool(prefab, randPosition);
        var orb = networkObj.GetComponent<TOrb>();

        if (orb == null)
        {
            Debug.LogError($"Failed to get {typeof(TOrb).Name} component from {networkObj.gameObject.name}.");
            return null; // Early exit if the component is missing
        }

        orb.Init(size);

        if (!networkObj.IsSpawned)
        {
            networkObj.Spawn();
        }

        // Safely add to the dictionary
        if (!m_prefabByInstanceMap.ContainsKey(orb.gameObject))
        {
            m_prefabByInstanceMap.Add(orb.gameObject, prefab);
        }
        else
        {
            Debug.LogWarning($"Duplicate key detected for {orb.gameObject.name}. Skipping addition.");
        }

        return orb;
    }

    private void GenerateCannister(GameObject prefab, Vector3 position, float rand = 0.3f)
    {
        var deltaX = Random.Range(-rand, rand);
        var deltaY = Random.Range(-rand, rand);
        var randPosition = position + new Vector3(deltaX, deltaY, 0);

        var networkObj = GetFromPool(prefab, randPosition);

        if (!networkObj.IsSpawned)
        {
            networkObj.Spawn();
        }

        // Safely add to the dictionary
        if (!m_prefabByInstanceMap.ContainsKey(networkObj.gameObject))
        {
            m_prefabByInstanceMap.Add(networkObj.gameObject, prefab);
        }
        else
        {
            Debug.LogWarning($"Duplicate key detected for {networkObj.gameObject.name}. Skipping addition.");
        }
    }

    private NetworkObject GetFromPool(GameObject prefab, Vector3 randPosition)
    {
        var networkObj = NetworkObjectPool.Instance.GetNetworkObject(prefab, randPosition, Quaternion.identity);

        if (networkObj == null)
        {
            Debug.LogError($"Failed to retrieve NetworkObject from pool for prefab: {prefab.name}");
        }

        return networkObj;
    }

    private void GenerateCGHSTOrb(Size size, Vector3 position, float rand = 0.5f)
    {
        GenerateOrb<CGHSTOrb>(CGHSTOrbPrefab, size, position, rand);
    }

    public void ReturnToPool(PickupItem item)
    {
        if (!IsServer)
        {
            return;
        }

        if (item == null || item.gameObject == null)
        {
            Debug.LogError("Attempted to return a null item to the pool.");
            return;
        }

        if (m_prefabByInstanceMap.TryGetValue(item.gameObject, out var prefab))
        {
            var networkObj = item.GetComponent<NetworkObject>();

            if (networkObj != null)
            {
                networkObj.gameObject.SetActive(false);
                networkObj.Despawn(false);

                Debug.Log("ReturnToPool - ReturnNetworkObject()");
                NetworkObjectPool.Instance.ReturnNetworkObject(networkObj, prefab);
                m_prefabByInstanceMap.Remove(item.gameObject);
            }
            else
            {
                Debug.LogWarning($"NetworkObject component missing on {item.gameObject.name}. Cannot return to pool.");
            }
        }
        else
        {
            //Debug.LogWarning($"Attempted to return {item.gameObject.name}, but it was not found in m_prefabByInstanceMap.");
        }
    }
}