using System.Collections.Generic;
using UnityEngine;

public class CrystalPlatformButtonGroupSpawner : MonoBehaviour
{
    [Header("General")]
    public int spawnerId = -1;
    public CrystalPlatformType CrystalPlatformType = CrystalPlatformType.Triangle;
    public int NumberButtons = 1;
    public List<CrystalPlatformSpawner> CrystalPlatformSpawners = new List<CrystalPlatformSpawner>();

    [Header("Prefabs")]
    //public GameObject SunkenFloorButtonGroupPrefab;
    public GameObject CrystalPlatformButtonPrefab;
}
