using System.Collections.Generic;
using Chest;
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
                // REDUNDANT spawn factories
                //SunkenFloorFactory.CreateSunkenFloors(gameObject);
                //ApeDoorFactory.CreateApeDoors(gameObject);
                //NetworkObjectSpawnerFactory.CreateNetworkObjectSpawners(gameObject, ref m_spawnerActivators);
                //CreateNetworkObjectPrefabSpawners();    // refer NetworkLevel_NetworkObjectPrefabSpawners.cs
                //NetworkObjectPrefabSpawnerFactory.CreateNetworkObjectPrefabSpawners(gameObject);
//<<<<<<< HEAD
//=======
                //TrapsGroupSpawnerFactory.CreateTraps(gameObject);
//>>>>>>> 6f6d2b82 ([ADD] chests logic)

                // legacy spawn factories to be replaced one day
                SubLevelFactory.CreateSubLevels(gameObject);
                TrapsGroupSpawnerFactory.CreateTraps(gameObject);


                // UPDATE: simplified "create" game logic
                CreateSpawners_ApeDoorsAndButtons();
                CreateSpawners_SunkenFloorsAndButtons();
                CreateSpawners_NetworkObject_v2();
                CreateSpawners_SpawnOnDestroyGroup();

                // reduce the level spawn count
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
            // UPDATE: simplified cleanup
            DestroySpawnerObjects<ApeDoorSpawner>();
            DestroySpawnerObjects<ApeDoorButtonGroupSpawner>();
            DestroySpawnerObjects<SunkenFloorSpawner>();
            DestroySpawnerObjects<SunkenFloorButtonGroupSpawner>();
            DestroySpawnerObjects<NetworkObjectPrefabSpawner>();
//<<<<<<< HEAD
            DestroySpawnerObjects<Spawner_NetworkObject_v2>();
            DestroySpawnerObjects<Spawner_SpawnOnDestroyGroup>();
            DestroySpawnerObjects<SunkenFloor3x3Spawner>();
            DestroySpawnerObjects<NetworkObjectSpawner>();
            DestroySpawnerObjects<TrapsGroupSpawner>();

            // destroy client side spawn points if not the host
            if (!IsHost) DestroySpawnerObjects<PlayerSpawnPoints>();
//=======
            //CleanupFactory.DestroySpawnerObjects<NetworkObjectPrefabSpawner>(gameObject);
            //CleanupFactory.DestroySpawnerObjects<TrapsGroupSpawner>(gameObject);
//>>>>>>> 6f6d2b82 ([ADD] chests logic)
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