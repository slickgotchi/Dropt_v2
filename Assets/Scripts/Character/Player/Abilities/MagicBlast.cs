using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class MagicBlast : PlayerAbility
{
    [Header("MagicBlast Parameters")]
    [SerializeField] float Projection = 0f;
    [SerializeField] int NumberHits = 4;

    private Collider2D m_collider;

    private float m_hitInterval;
    private float m_hitTimer;
    private int m_hitCounter;

    public override void OnNetworkSpawn()
    {
        m_collider = GetComponent<Collider2D>();

        m_hitInterval = ExecutionDuration / (NumberHits - 1);
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        m_hitCounter = NumberHits;
        m_hitTimer = m_hitInterval;

        CollisionCheck();
        m_hitCounter--;

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        PlayAnimation("MagicBlast");
    }

    private void CollisionCheck()
    {
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Magic, DamageMultiplier);
    }

    public override void OnUpdate()
    {
        // update hit timer and if need to do another collision, do it
        m_hitTimer -= Time.deltaTime;
        if (m_hitCounter > 0 && m_hitTimer <= 0)
        {
            CollisionCheck();
            m_hitTimer = m_hitInterval;
            m_hitCounter--;
        }
    }

    public override void OnFinish()
    {
        while (m_hitCounter > 0)
        {
            CollisionCheck();
            m_hitCounter--;
        }
    }
}
