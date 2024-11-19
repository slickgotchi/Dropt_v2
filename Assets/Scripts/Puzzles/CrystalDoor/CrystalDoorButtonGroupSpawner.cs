using System.Collections.Generic;
using UnityEngine;

public class CrystalDoorButtonGroupSpawner : MonoBehaviour
{
    [Header("General")]
    public int spawnerId = -1;
    public CrystalDoorType CrystalDoorType = CrystalDoorType.R;
    public int NumberButtons = 1;
    public List<CrystalDoorSpawner> CrystalDoorSpawners = new List<CrystalDoorSpawner>();

    //public int OpenDoorsWithSpawnerId = -1;

    [Header("Prefabs")]
    //public GameObject ApeDoorButtonGroupPrefab;
    public GameObject CrystalDoorButtonPrefab;

#if UNITY_EDITOR
    private void Awake()
    {
        if (CrystalDoorSpawners.Count == 0)
        {
            Debug.LogWarning("CrystalDoorSpawners Has Not Been Assign To Crystal Door Button Group Spawner", this);
        }
    }
#endif
}
