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

        orb.Init(size);

        if (!networkObj.IsSpawned)
        {
            networkObj.Spawn();
        }

        m_prefabByInstanceMap.Add(orb.gameObject, prefab);
        return orb;
    }

    private NetworkObject GetFromPool(GameObject prefab, Vector3 randPosition)
    {
        return NetworkObjectPool.Instance
            .GetNetworkObject(prefab, randPosition, Quaternion.identity);
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

        if (m_prefabByInstanceMap.TryGetValue(item.gameObject, out var prefab))
        {
            var networkObj = item.GetComponent<NetworkObject>();
            networkObj.gameObject.SetActive(false);
            networkObj.Despawn(false);

            NetworkObjectPool.Instance.ReturnNetworkObject(networkObj, prefab);
            m_prefabByInstanceMap.Remove(item.gameObject);
        }
    }
}