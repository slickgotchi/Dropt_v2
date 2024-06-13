using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs_NetworkObject_Enemy : MonoBehaviour
{
    public GameObject BombSnail;
    public GameObject FudSpirit;
    public GameObject FudWisp;
    public GameObject FussPot;
    public GameObject GasBag;
    public GameObject GeodeShade;
    public GameObject LeafShade;
    public GameObject SentryBot;
    public GameObject Snail;
    public GameObject Spider;
    public GameObject SpiderPod;
    public GameObject WispHollow;

    public static Prefabs_NetworkObject_Enemy Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
