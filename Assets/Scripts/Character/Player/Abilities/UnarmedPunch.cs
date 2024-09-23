using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class UnarmedPunch : PlayerAbility
{
    [Header("UnarmedPunch Parameters")]
    [SerializeField] float Projection = 1f;

    private Collider2D m_collider;

    private bool m_isCollisionChecked = false;

    private float m_unarmedPunchTimer = 0f;

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

        m_unarmedPunchTimer = 0;

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        PlayAnimation("UnarmedPunch");
    }

    public override void OnUpdate()
    {
        if (!IsActivated) return;

        m_unarmedPunchTimer += Time.deltaTime;

        if (m_unarmedPunchTimer > ExecutionDuration*0.8f && !m_isCollisionChecked)
        {
            // collision check (no RPC's are involved in this call)
            OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Unarmed);

            m_isCollisionChecked = true;
        }
    }

    public override void OnFinish()
    {
        m_isCollisionChecked = false;
    }
}
