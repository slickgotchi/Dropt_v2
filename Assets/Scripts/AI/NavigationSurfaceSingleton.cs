using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSurfaceSingleton : MonoBehaviour
{
    public static NavigationSurfaceSingleton Instance { get; private set; }

    public NavMeshSurface Surface { get; private set; }

    private void Awake()
    {
        Instance = this; 
        Surface = GetComponent<NavMeshSurface>();
    }


}
