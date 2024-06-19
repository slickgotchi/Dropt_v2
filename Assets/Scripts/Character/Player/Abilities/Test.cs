using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject dashPrefab;

    private void Start()
    {
        var dash = Instantiate(dashPrefab);
        dash.GetComponent<NetworkObject>().Spawn();
    }
}
