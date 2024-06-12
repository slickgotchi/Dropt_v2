using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubLevelSpawner : MonoBehaviour
{
    public List<SpawnSubLevelGameObject> SubLevels;
}

[System.Serializable]
public class SpawnSubLevelGameObject
{
    public GameObject SubLevelPrefab;
    public float SpawnChance;
}
