using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs_NetworkObject_Interactable : MonoBehaviour
{
    public GameObject Hole;
    public GameObject GotchiSelectPortal;
    public GameObject EscapePortal;

    public static Prefabs_NetworkObject_Interactable Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
