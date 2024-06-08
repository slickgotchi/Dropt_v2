using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApeDoorSpawner : MonoBehaviour
{
    [Header("General")]
    public ApeDoorType ApeDoorType = ApeDoorType.Crescent;
    public int NumberButtons = 1;

    [Header("Prefabs")]
    public GameObject ApeDoorPrefab;
    public GameObject ApeDoorButtonPrefab;
}
