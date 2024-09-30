using UnityEngine;

public class Spawner_SpawnCondition : MonoBehaviour
{
    [SerializeField]
    public Level.LevelSpawn.SpawnCondition spawnCondition = Level.LevelSpawn.SpawnCondition.ElapsedTime;

    // ElapsedTime
    [SerializeField]
    public float elapsedTime = 0f;

    // DestroyedAllWithSpawnerId
    [SerializeField]
    public int destroyAllWithSpawnerId = -1;

    // PlayerTouchTriggerWithSpawnerId
    [SerializeField]
    public int touchTriggerWithSpawnerId = -1;
}
