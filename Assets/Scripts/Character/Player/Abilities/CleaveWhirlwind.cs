using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class CleaveWhirlwind : PlayerAbility
{
    [Header("CleaveWhirlwind Parameters")]
    [SerializeField] private int NumberHits = 3;

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
        SetScale(math.min(1 + (HoldDuration / HoldChargeTime), 2f));

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
        while (m_hitCounter > 0)
        {
            CollisionCheck();
            m_hitCounter--;
        }

        PlayAnimation("CleaveDefault");
    }
}
