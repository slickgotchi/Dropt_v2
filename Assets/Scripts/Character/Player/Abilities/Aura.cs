using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class Aura : PlayerAbility
{
    [Header("Aura Parameters")]
    [SerializeField] float Projection = 1f;


    public override void OnNetworkSpawn()
    {
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        PlayAnimation("Aura");
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {

    }
}
