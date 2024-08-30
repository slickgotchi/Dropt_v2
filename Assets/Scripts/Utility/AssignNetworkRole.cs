using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignNetworkRole : MonoBehaviour
{
    public NetworkRole networkRole = NetworkRole.Host;

    private void Awake()
    {
        if (networkRole == NetworkRole.Host)
        {
            // do nothing
        }
        else if (networkRole == NetworkRole.Client)
        {
            if (Bootstrap.IsServer()) Destroy(this.gameObject);
        }
        else if (networkRole == NetworkRole.Server)
        {
            if (Bootstrap.IsClient()) Destroy(this.gameObject);
        }
    }
}
