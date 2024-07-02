using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class CleaveSwing : PlayerAbility
{
    [Header("CleaveSwing Parameters")]
    [SerializeField] float Projection = 1f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // collision check (no RPC's are involved in this call)
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Cleave);

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        PlayAnimation("CleaveSwing");


    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {

    }
}
