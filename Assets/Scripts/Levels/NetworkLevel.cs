using System.Collections.Generic;
using Level.Traps;
using Unity.Netcode;
using UnityEngine;

namespace Level
{
    public partial class NetworkLevel : NetworkBehaviour
    {
        public enum LevelType { Null, Tutorial, DegenapeVillage, Dungeon, DungeonRest }
        public LevelType levelType = LevelType.Dungeon;

        public AudioClip levelMusic;
        public bool isEssenceDepleting = true;
        public string objective = "Find a hole to descend";

        public enum NavmeshGeneration { RenderMeshes, PhysicsColliders }
        public NavmeshGeneration navmeshGeneration = NavmeshGeneration.PhysicsColliders;

        //private List<Vector3> m_availablePlayerSpawnPoints = new List<Vector3>();
        private List<SpawnerActivator> m_spawnerActivators = new List<SpawnerActivator>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Random.InitState(System.DateTime.Now.Millisecond);

            if (IsServer)
            {
                // legacy spawn factories to be replaced one day
                SubLevelFactory.CreateSubLevels(gameObject);
                TrapsGroupSpawnerFactory.CreateTraps(gameObject);

                // UPDATE: simplified "create" game logic
                CreateSpawners_ApeDoorsAndButtons();
                CreateSpawners_CrystalDoorsAndButtons();
                CreateSpawners_SunkenFloorsAndButtons();
                CreateSpawners_NetworkObject_v2();
                CreateSpawners_SpawnOnDestroyGroup();

                // reduce the level spawn count
                LevelManager.Instance.LevelSpawningCount--;
            }

            if (IsClient)
            {
                CleanupSpawnerObjects();
                AudioManager.Instance.CrossfadeMusic(levelMusic == null ? AudioLibrary.Instance.UndergroundForest : levelMusic);
            }
        }

        public override void OnNetworkDespawn()
        {
            // Implement any necessary cleanup here
            base.OnNetworkDespawn();
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
            DestroySpawnerObjects<CrystalDoorSpawner>();
            DestroySpawnerObjects<CrystalDoorButtonGroupSpawner>();
            DestroySpawnerObjects<SunkenFloorSpawner>();
            DestroySpawnerObjects<SunkenFloorButtonGroupSpawner>();
            DestroySpawnerObjects<NetworkObjectPrefabSpawner>();
            DestroySpawnerObjects<Spawner_NetworkObject_v2>();
            DestroySpawnerObjects<Spawner_SpawnOnDestroyGroup>();
            DestroySpawnerObjects<SunkenFloor3x3Spawner>();
            DestroySpawnerObjects<NetworkObjectSpawner>();
            DestroySpawnerObjects<TrapsGroupSpawner>();

            // destroy client side spawn points if not the host
            if (!IsHost) DestroySpawnerObjects<PlayerSpawnPoints>();

        }

        public void DestroySpawnerObjects<T>() where T : Component
        {
            List<T> spawnerObjects = new List<T>(GetComponentsInChildren<T>());
            foreach (var spawnerObject in spawnerObjects)
            {
                Destroy(spawnerObject.gameObject);
            }
        }

        public void DestroyAllChildren(Transform parent)
        {
            while (parent.childCount > 0)
            {
                Transform child = parent.GetChild(0);
                child.parent = null;
                Destroy(child.gameObject);
            }
        }
    }
}