using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace Core.Pool
{
    /// <summary>
    /// Object Pool for networked objects, used for controlling how objects are spawned by Netcode. Netcode by default
    /// will allocate new memory when spawning new objects. With this Networked Pool, we're using the ObjectPool to
    /// reuse objects.
    /// Boss Room uses this for projectiles. In theory it should use this for imps too, but we wanted to show vanilla spawning vs pooled spawning.
    /// Hooks to NetworkManager's prefab handler to intercept object spawning and do custom actions.
    /// </summary>
    public class NetworkObjectPool : NetworkBehaviour
    {
        public static NetworkObjectPool Instance { get; private set; }

        //[SerializeField] List<PoolConfigObject> m_poolConfigObjects;
        [SerializeField] List<GameObject> m_poolPrefabs;
        [SerializeField] int m_poolPrefabPrewarmCount = 1;

        HashSet<GameObject> m_Prefabs = new HashSet<GameObject>();

        Dictionary<GameObject, ObjectPool<NetworkObject>> m_PooledObjects =
            new Dictionary<GameObject, ObjectPool<NetworkObject>>();

        public void Awake()
        {
            // Singleton pattern to ensure only one instance of the AudioManager exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            foreach (var poolPrefab in m_poolPrefabs)
            {
                RegisterPrefabInternal(poolPrefab, m_poolPrefabPrewarmCount);
            }

            // Registers all objects in cust prefab pool to the cache.
            //foreach (var configObject in m_poolConfigObjects)
            //{
            //    RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
            //}
        }

        public override void OnNetworkDespawn()
        {
            // Unregisters all objects in PooledPrefabsList from the cache.
            foreach (var prefab in m_Prefabs)
            {
                // Unregister Netcode Spawn handlers
                NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
                m_PooledObjects[prefab].Clear();
            }

            m_PooledObjects.Clear();
            m_Prefabs.Clear();

            base.OnNetworkDespawn();
        }

        public void OnValidate()
        {
            for (var i = 0; i < m_poolPrefabs.Count; i++)
            {
                var prefab = m_poolPrefabs[i];
                if (prefab != null)
                {
                    Assert.IsNotNull(prefab.GetComponent<NetworkObject>(),
                        $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
                }
            }

            //for (var i = 0; i < m_poolConfigObjects.Count; i++)
            //{
            //    var prefab = m_poolConfigObjects[i].Prefab;
            //    if (prefab != null)
            //    {
            //        Assert.IsNotNull(prefab.GetComponent<NetworkObject>(),
            //            $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            //    }
            //}
        }

        /// <summary>
        /// Gets a NetworkObject instance of the given prefab from the pool. The prefab must be registered to the pool.
        /// </summary>
        /// <remarks>
        /// To spawn a NetworkObject from one of the pools, this must be called on the server, then the instance
        /// returned from it must be spawned on the server. This method will then also be called on the client by the
        /// PooledPrefabInstanceHandler when the client receives a spawn message for a prefab that has been registered
        /// here.
        /// </remarks>
        /// <param name="prefab"></param>
        /// <param name="position">The position to spawn the object at.</param>
        /// <param name="rotation">The rotation to spawn the object with.</param>
        /// <returns></returns>
        public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            // Check if the prefab is registered in the pool
            if (!m_PooledObjects.ContainsKey(prefab))
            {
                Debug.LogError($"GetNetworkObject(): Prefab {prefab.name} not found in the pool. Creating a new instance dynamically and returning to caller.");

                return null;
            }

            //Debug.Log($"GetNetworkObject(): Prefab {prefab.name} found in pool and returning to caller");

            // Retrieve an instance from the pool
            var pooledNetworkObject = m_PooledObjects[prefab].Get();
            pooledNetworkObject.transform.position = position;
            pooledNetworkObject.transform.rotation = rotation;

            return pooledNetworkObject;
        }

        /// <summary>
        /// Return an object to the pool (reset objects before returning).
        /// </summary>
        public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
        {
            //Debug.Log($"ReturnNetworkObject {networkObject.name}, {prefab.name}");
            if (!m_PooledObjects.ContainsKey(prefab))
            {
                Debug.LogError($"ReturnNetworkObject(): Prefab {prefab.name} not found in the pool.");
                return;
            }

            try
            {
                m_PooledObjects[prefab].Release(networkObject);
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogError($"ReturnNetworkObject(): Pooled object release failed {prefab.name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds up the cache for a prefab.
        /// </summary>
        void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
        {
            NetworkObject CreateFunc()
            {
                var networkObject = Instantiate(prefab).GetComponent<NetworkObject>();
                networkObject.gameObject.SetActive(false);
                return networkObject;
            }

            void ActionOnGet(NetworkObject networkObject)
            {
                if (networkObject == null) return;

                // set pooled object in use
                var pooledObject = networkObject.GetComponent<PooledObject>();
                if (pooledObject != null) pooledObject.IsInUse = true;

                // activate gameobject
                networkObject.gameObject.SetActive(true);
            }

            void ActionOnRelease(NetworkObject networkObject)
            {
                if (networkObject == null) return;

                // set pooled object not in use
                var pooledObject = networkObject.GetComponent<PooledObject>();
                if (pooledObject != null) pooledObject.IsInUse = false;

                // activate gameobject
                networkObject.gameObject.SetActive(false);
            }

            void ActionOnDestroy(NetworkObject networkObject)
            {
                if (networkObject == null) return;
                Destroy(networkObject.gameObject);
            }

            m_Prefabs.Add(prefab);

            if (!prefab.HasComponent<PooledObject>())
            {
                Debug.LogError($"RegistarPrefabInternal: {prefab.name} does not have a PooledObject component attached to it!");
            }

            // Create the pool
            m_PooledObjects[prefab] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease,
                ActionOnDestroy, defaultCapacity: prewarmCount);

            // Populate the pool
            var prewarmNetworkObjects = new List<NetworkObject>();
            for (var i = 0; i < prewarmCount; i++)
            {
                var obj = m_PooledObjects[prefab].Get();

                // Ensure prewarmed objects are inactive
                obj.gameObject.SetActive(false);

                prewarmNetworkObjects.Add(obj);
            }

            foreach (var networkObject in prewarmNetworkObjects)
            {
                m_PooledObjects[prefab].Release(networkObject);
            }

            // Register Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
        }
    }

    [Serializable]
    struct PoolConfigObject
    {
        public GameObject Prefab;
        public int PrewarmCount;
    }

    class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        GameObject m_Prefab;
        NetworkObjectPool m_Pool;

        public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
        {
            m_Prefab = prefab;
            m_Pool = pool;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position,
            Quaternion rotation)
        {
            return m_Pool.GetNetworkObject(m_Prefab, position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            m_Pool.ReturnNetworkObject(networkObject, m_Prefab);
        }
    }
}