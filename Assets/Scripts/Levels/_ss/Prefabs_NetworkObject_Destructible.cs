using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs_NetworkObject_Destructible : MonoBehaviour
{
    public GameObject BlueCrate;
    public GameObject BlueFern;
    public GameObject Geode_Plain;
    public GameObject Geode_Common;
    public GameObject Geode_Uncommon;
    public GameObject Geode_Rare;
    public GameObject Geode_Legendary;
    public GameObject Geode_Mythical;
    public GameObject Geode_Godlike;


    public static Prefabs_NetworkObject_Destructible Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
