using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CrystalDoorButtonGroup : NetworkBehaviour
{
    public NetworkVariable<int> NumberButtons = new NetworkVariable<int>();
    public int initNumberButtons = 2;
    public int spawnerId = -1;

    [HideInInspector] public List<GameObject> CrystalDoors = new List<GameObject>();

    private void Awake()
    {
        if (CrystalDoors.Count == 0)
        {
            Debug.LogWarning("CrystalDoorButtonGroup does not have a door assigned to it", this);
        }
    }
}
