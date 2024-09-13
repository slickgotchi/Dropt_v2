using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ApeDoorButtonGroup : NetworkBehaviour
{
    [Header("State")]
    //public NetworkVariable<ApeDoorType> Type;
    //public NetworkVariable<DoorState> State;
    public NetworkVariable<int> NumberButtons = new NetworkVariable<int>();
    public int initNumberButtons = 2;
    public int spawnerId = -1;

    [HideInInspector] public List<GameObject> ApeDoors = new List<GameObject>();
}
