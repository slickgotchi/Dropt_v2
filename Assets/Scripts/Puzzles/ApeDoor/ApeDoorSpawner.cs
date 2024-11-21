using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApeDoorSpawner : MonoBehaviour
{
    [HideInInspector] public int spawnerId = -1;
    public GameObject ApeDoorPrefab;

#if UNITY_EDITOR
    private void Awake()
    {
        if (ApeDoorPrefab == null)
        {
            Debug.LogWarning("DOOR PREFAB IS NOT ASSIGN TO APE DOOR SPAWNER", this);
        }
    }
#endif
}
