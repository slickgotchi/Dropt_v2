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
    public float spawnTimeAfterDestroyAll = 0f;

    // PlayerTouchTriggerWithSpawnerId
    [SerializeField]
    [Tooltip("The ID of the SpawnTrigger that is linked to this spawner")]
    public int touchTriggerWithSpawnerId = -1;
    public float spawnTimeAfterTrigger = 0f;
}
