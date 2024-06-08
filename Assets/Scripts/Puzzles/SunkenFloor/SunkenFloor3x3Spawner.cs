using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunkenFloor3x3Spawner : MonoBehaviour
{
    [Header("General")]
    public SunkenFloorType SunkenFloorType = SunkenFloorType.Droplet;
    public int NumberButtons = 1;

    [Header("Prefabs")]
    public GameObject SunkenFloor3x3Prefab;
    public GameObject SunkenFloorButtonPrefab;
}
