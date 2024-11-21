using System.Collections.Generic;
using UnityEngine;

public class SunkenFloorButtonGroupSpawner : MonoBehaviour
{
    [Header("General")]
    public int spawnerId = -1;
    public SunkenFloorType SunkenFloorType = SunkenFloorType.Droplet;
    public int NumberButtons = 1;
    public List<SunkenFloorSpawner> SunkenFloorSpawners = new List<SunkenFloorSpawner>();

    [Header("Prefabs")]
    //public GameObject SunkenFloorButtonGroupPrefab;
    public GameObject SunkenFloorButtonPrefab;
}