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
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Surface = GetComponent<NavMeshSurface>();
    }


}
