using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Level
{
    public class NetworkLevel : NetworkBehaviour
    {
        private List<Vector3> m_availablePlayerSpawnPoints = new List<Vector3>();
        private List<SpawnerActivator> m_spawnerActivators = new List<SpawnerActivator>();

        public override void OnNetworkSpawn()
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

            if (IsServer)
            {
                if (LevelManager.Instance.LevelSpawningCount == 1)
                {
                    m_availablePlayerSpawnPoints.Clear();
                }

                SunkenFloorFactory.CreateSunkenFloors(gameObject);
                ApeDoorFactory.CreateApeDoors(gameObject);
                NetworkObjectSpawnerFactory.CreateNetworkObjectSpawners(gameObject, ref m_spawnerActivators);
                SubLevelFactory.CreateSubLevels(gameObject);
                NetworkObjectPrefabSpawnerFactory.CreateNetworkObjectPrefabSpawners(gameObject);

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
            CleanupFactory.DestroySpawnerObjects<ApeDoorSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<ApeDoorButtonGroupSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<NetworkObjectSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<PlayerSpawnPoints>(gameObject);
            CleanupFactory.DestroySpawnerObjects<SunkenFloorSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<SunkenFloorButtonGroupSpawner>(gameObject);
            CleanupFactory.DestroySpawnerObjects<NetworkObjectPrefabSpawner>(gameObject);
        }

        public Vector3 PopPlayerSpawnPoint()
        {
            if (m_availablePlayerSpawnPoints.Count > 0)
            {
                var randIndex = UnityEngine.Random.Range(0, m_availablePlayerSpawnPoints.Count);
                var spawnPoint = m_availablePlayerSpawnPoints[randIndex];
                m_availablePlayerSpawnPoints.RemoveAt(randIndex);
                return spawnPoint;
            }

            Debug.Log("NetworkLevel: Ran out of spawn points! Returning Vector3.zero");
            return Vector3.zero;
        }
    }
}
