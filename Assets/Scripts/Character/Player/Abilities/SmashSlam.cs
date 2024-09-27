using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class SmashSlam : PlayerAbility
{
    [Header("SmashSlam Parameters")]

    private Collider2D m_collider;

    private float m_colliderTimer = 0f;
    private bool m_isCollisionChecked = false;

    public override void OnNetworkSpawn()
    {
        m_collider = GetComponentInChildren<Collider2D>();
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
        PlayAnimation("SmashSlam");
        Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchi_SmashSlam");

        m_colliderTimer = ExecutionDuration * 0.9f; // we use this collider timer so the knockback occurs towards end of attack
        m_isCollisionChecked = false;
    }

    public override void OnUpdate()
    {
        m_colliderTimer -= Time.deltaTime;
        if (!m_isCollisionChecked && m_colliderTimer <= 0f)
        {
            OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Smash, DamageMultiplier);
            m_isCollisionChecked = true;
        }
    }

    public override void OnFinish()
    {
        if (!m_isCollisionChecked)
        {
            OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Smash, DamageMultiplier);
            m_isCollisionChecked = true;
        }
        Player.GetComponent<PlayerGotchi>().ResetIdleAnimation();
    }

    
}
