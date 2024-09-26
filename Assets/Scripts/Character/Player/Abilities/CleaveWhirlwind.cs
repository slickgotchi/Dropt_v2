using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class CleaveWhirlwind : PlayerAbility
{
    [Header("CleaveWhirlwind Parameters")]
    [SerializeField] private int NumberHits = 3;

    public float m_holdStartScale = 1f;
    public float m_holdFinishScale = 2f;

    private float m_hitInterval;
    private float m_hitTimer;
    private int m_hitCounter;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        m_collider = GetComponent<Collider2D>();

        m_hitInterval = ExecutionDuration / (NumberHits - 1);
    }

    public override void OnStart()
    {
        // set transform to activation rotation/position and scale based on hold duration
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);
        var alpha = math.min(m_holdTimer / HoldChargeTime, 1);
        SetScale(math.lerp(m_holdStartScale, m_holdFinishScale, alpha));

        m_hitCounter = NumberHits;
        m_hitTimer = m_hitInterval;

        CollisionCheck();
        m_hitCounter--;

        PlayAnimation("CleaveWhirlwind");

        Player.GetComponent<PlayerGotchi>().PlayFacingSpin(3, ExecutionDuration / 3,
            PlayerGotchi.SpinDirection.AntiClockwise, 0);

    }

    private void CollisionCheck()
    {
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Cleave, DamageMultiplier);
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
        m_holdTimer = 0;
        while (m_hitCounter > 0)
        {
            CollisionCheck();
            m_hitCounter--;
        }

        PlayAnimation("CleaveDefault");
    }
}
