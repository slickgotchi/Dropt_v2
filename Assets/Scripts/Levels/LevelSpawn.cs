using UnityEngine;

namespace Level
{
    public class LevelSpawn : MonoBehaviour
    {
        // spawn conditions
        public enum SpawnCondition
        {
            ElapsedTime,
            PlayerDestroyAllWithSpawnerId,
            PlayerTouchTriggerWithSpawnerId,
        }

        // variables set in inspector
        public int spawnerId;
        public LevelSpawn.SpawnCondition spawnCondition;
        public float elapsedTime = 0f;
        public float spawnInterval = 5f;
        public int destroyAllWithSpawnerId = -1;
        public float spawnTimeAfterDestroyAll = 0;
        public int touchTriggerWithSpawnerId = -1;
        public float spawnTimeAfterTrigger = 0;

        // variables set by logic
        public bool isSpawned = false;
        public bool isTouchedByPlayer = false;

        // the prefab used to create this level spawn
        [HideInInspector] public GameObject prefab;

        // take note when a player touches this object
        private void OnTriggerEnter2D(Collider2D collision)
        {
            isTouchedByPlayer = true;
        }
    }
}
