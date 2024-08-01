using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// If we are a server, this gameobject is destroyed

public class DestroyIfServer : MonoBehaviour
{
    private void Start()
    {
        if (Bootstrap.IsServer()) Destroy(gameObject);
    }
}
