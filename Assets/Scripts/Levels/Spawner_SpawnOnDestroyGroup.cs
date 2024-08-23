using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner_SpawnOnDestroyGroup : MonoBehaviour
{
    public GameObject SpawnOnDestroyPrefab;
    public Vector3 Offset;
    public float SpawnChance = 0.1f;
    public int MinNumberSpawns = 1;
    public int MaxNumberSpawns = 2;
}
