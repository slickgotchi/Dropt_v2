using System.Collections.Generic;
using Level.Traps;
using Unity.Netcode;
using UnityEngine;

namespace Level
{
    public partial class NetworkLevel : NetworkBehaviour
    {
        //private List<Vector3> m_availablePlayerSpawnPoints = new List<Vector3>();
        private List<SpawnerActivator> m_spawnerActivators = new List<SpawnerActivator>();

        public override void OnNetworkSpawn()
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);


            if (IsServer)
            {
                SunkenFloorFactory.CreateSunkenFloors(gameObject);


                //ApeDoorFactory.CreateApeDoors(gameObject);
                NetworkObjectSpawnerFactory.CreateNetworkObjectSpawners(gameObject, ref m_spawnerActivators);
                SubLevelFactory.CreateSubLevels(gameObject);
                CreateNetworkObjectPrefabSpawners();    // refer NetworkLevel_NetworkObjectPrefabSpawners.cs
                //NetworkObjectPrefabSpawnerFactory.CreateNetworkObjectPrefabSpawners(gameObject);
                TrapsGroupSpawnerFactory.CreateTraps(gameObject);


                // UPDATE: simplified "create" game logic
                CreateSpawners_ApeDoorsAndButtons();
                CreateSpawners_NetworkObject_v2();

                LevelManager.Instance.LevelSpawningCount--;
            }

            if (IsClient)
            {
                CleanupSpawnerObjects();
            }
        }

        public override void OnNetworkDespawn()
        {
            // Implement any necessary cleanup here
        }

        private void Update()
        {
            if (!IsServer) return;

            foreach (var spawnerActivator in m_spawnerActivators)
            {
                spawnerActivator.Update(Time.deltaTime);
            }
        }

        private void CleanupSpawnerObjects()
        {
            CleanupFactory.DestroySpawnerObjects<SunkenFloor3x3Spawner>(gameObject);
            //CleanupFactory.DestroySpawnerObjects<ApeDoorSpawner>(gameObject);
            //CleanupFactory.DestroySpawnerObjects<ApeDoorButtonGroupSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<NetworkObjectSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<PlayerSpawnPoints>(gameObject);
            CleanupFactory.DestroySpawnerObjects<SunkenFloorSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<SunkenFloorButtonGroupSpawner>(gameObject);
            //CleanupFactory.DestroySpawnerObjects<NetworkObjectPrefabSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<TrapsGroupSpawner>(gameObject);

            // UPDATE: simplified cleanup
            DestroySpawnerObjects<ApeDoorSpawner>();
            DestroySpawnerObjects<ApeDoorButtonGroupSpawner>();
            DestroySpawnerObjects<NetworkObjectPrefabSpawner>();
            DestroySpawnerObjects<Spawner_NetworkObject_v2>();
        }

        public void DestroySpawnerObjects<T>() where T : Component
        {
            var spawnerObjects = new List<T>(GetComponentsInChildren<T>());
            foreach (var spawnerObject in spawnerObjects)
            {
                Object.Destroy(spawnerObject.gameObject);
            }
        }

        public void DestroyAllChildren(Transform parent)
        {
            while (parent.childCount > 0)
            {
                var child = parent.GetChild(0);
                child.parent = null;
                Object.Destroy(child.gameObject);
            }
        }
    }
}
