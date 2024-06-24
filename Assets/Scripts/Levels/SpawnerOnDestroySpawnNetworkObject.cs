using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerOnDestroySpawnNetworkObject : MonoBehaviour
{
    public enum EnumType { Chance, Range }
    public EnumType Type;

    public float Chance = 0.1f;
    public int MinRange = 1;
    public int MaxRange = 2;

    public GameObject NetworkObjectPrefab;
}
