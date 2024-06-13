using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs_NetworkObject_Prop : MonoBehaviour
{
    public GameObject LightPole;
    public GameObject RuinBlock;

    public static Prefabs_NetworkObject_Prop Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
