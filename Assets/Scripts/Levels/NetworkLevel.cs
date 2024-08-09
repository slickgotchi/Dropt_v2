using System.Collections.Generic;
using Level.Traps;
using Unity.Netcode;
using UnityEngine;

namespace Level
{
    public class NetworkLevel : NetworkBehaviour
    {
        //private List<Vector3> m_availablePlayerSpawnPoints = new List<Vector3>();
        private List<SpawnerActivator> m_spawnerActivators = new List<SpawnerActivator>();

        public override void OnNetworkSpawn()
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

            ProgressBarCanvas.Instance.Show("Spawning level...", 0.6f);

            if (IsServer)
            {
                SunkenFloorFactory.CreateSunkenFloors(gameObject);
                ApeDoorFactory.CreateApeDoors(gameObject);
                NetworkObjectSpawnerFactory.CreateNetworkObjectSpawners(gameObject, ref m_spawnerActivators);
                SubLevelFactory.CreateSubLevels(gameObject);
                NetworkObjectPrefabSpawnerFactory.CreateNetworkObjectPrefabSpawners(gameObject);
                TrapsGroupSpawnerFactory.CreateTraps(gameObject);

                LevelManager.Instance.LevelSpawningCount--;
            }

            if (IsClient)
            {
                CleanupSpawnerObjects();
            }

            ProgressBarCanvas.Instance.Show("Level spawn complete", 1f);
            ProgressBarCanvas.Instance.Hide();
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
            CleanupFactory.DestroySpawnerObjects<TrapsGroupSpawner>(gameObject);
        }
    }
}
