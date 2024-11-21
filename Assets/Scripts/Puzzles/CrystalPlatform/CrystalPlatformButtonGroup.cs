using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CrystalPlatformButtonGroup : MonoBehaviour
{
    [Header("State")]
    public NetworkVariable<int> NumberButtons = new NetworkVariable<int>();
    public int initNumberButtons = 2;
    public int spawnerId = -1;

    [HideInInspector] public List<GameObject> CrystalPlatforms = new List<GameObject>();
}