using System.Collections.Generic;
using UnityEngine;

public class ApeDoorButtonGroupSpawner : MonoBehaviour
{
    [Header("General")]
    public int spawnerId = -1;
    public ApeDoorType ApeDoorType = ApeDoorType.Crescent;
    public int NumberButtons = 1;
    public List<ApeDoorSpawner> ApeDoorSpawners = new List<ApeDoorSpawner>();

    //public int OpenDoorsWithSpawnerId = -1;

    [Header("Prefabs")]
    //public GameObject ApeDoorButtonGroupPrefab;
    public GameObject ApeDoorButtonPrefab;

#if UNITY_EDITOR
    private void Awake()
    {
        if (ApeDoorSpawners.Count == 0)
        {
            Debug.LogWarning("ApeDoorSpawners Has Not Been Assign To ApeDoor Button Group Spawner", this);
        }
    }
#endif
}
