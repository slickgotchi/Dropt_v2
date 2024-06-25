using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApeDoorButtonGroupSpawner : MonoBehaviour
{
    [Header("General")]
    public ApeDoorType ApeDoorType = ApeDoorType.Crescent;
    public int NumberButtons = 1;
    public List<ApeDoorSpawner> ApeDoorSpawners = new List<ApeDoorSpawner>();

    [Header("Prefabs")]
    public GameObject ApeDoorButtonGroupPrefab;
    public GameObject ApeDoorButtonPrefab;
}
