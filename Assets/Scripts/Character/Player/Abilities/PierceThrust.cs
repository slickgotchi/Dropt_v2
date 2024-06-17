using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PierceThrust : PlayerAbility
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("PierceThrust spawned " + IsServer);
    }
}
