using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ApeDoorButtonGroup : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<int> NumberButtons = new NetworkVariable<int>();
    public int initNumberButtons = 2;
    public int spawnerId = -1;

    [HideInInspector] public List<GameObject> ApeDoors = new List<GameObject>();

    private void Awake()
    {
        if (ApeDoors.Count == 0)
        {
            Debug.LogWarning("ApeDoorButtonGroup does not have a door assigned to it", this);
        }
    }
}
