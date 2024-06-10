using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityWorldSingleton : MonoBehaviour
{
    public static UtilityWorldSingleton Instance {  get; private set; }

    public UtilityWorldController World;

    private void Awake()
    {
        Instance = this;
        World = GetComponent<UtilityWorldController>();
    }
}
