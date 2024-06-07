using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunkenFloor3x3Spawner : MonoBehaviour
{
    public SunkenFloorType SunkenFloorType = SunkenFloorType.Droplet;

    [Header("Prefabs")]
    public GameObject SunkenFloor3x3Prefab;
    public GameObject SunkenFloorButtonPrefab;
}
