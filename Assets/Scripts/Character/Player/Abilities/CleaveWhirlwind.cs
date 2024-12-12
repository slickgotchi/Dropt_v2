using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class CleaveWhirlwind : PlayerAbility
{
    [Header("CleaveWhirlwind Parameters")]
    [SerializeField] private float m_hitInterval = 0.25f;
    [SerializeField] private float m_holdStartRadius = 2f;
    [SerializeField] private float m_holdFinishRadius = 5f;

    private float m_hitTimer = 0f;

    private Collider2D m_collider;

    private AttackPathVisualizer m_attackPathVisualizer;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // set transform to activation rotation/position and scale based on hold duration
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);

        // the base whirlwind anim sprite has 1.75 radius. So scale 1 == radius 1.75
        // thus to calc scale we go
        var targetRadius = math.lerp(m_holdStartRadius, m_holdFinishRadius, GetHoldPercentage());
        SetScale(targetRadius / 1.75f);

        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Cleave, DamageMultiplier);

        PlayAnimation("CleaveWhirlwind");

        Player.GetComponent<PlayerGotchi>().PlayFacingSpin(3, ExecutionDuration / 3,
            PlayerGotchi.SpinDirection.AntiClockwise, 0);
    }

    public override void OnUpdate()
    {
        // update hit timer and if need to do another collision, do it
        m_hitTimer -= Time.deltaTime;
        if (m_hitTimer <= 0)
        {
            OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Cleave, DamageMultiplier);
            m_hitTimer += m_hitInterval;
        }
    }


    public override void OnHoldStart()
    {
        base.OnHoldStart();

        if (Player == null) return;

        m_attackPathVisualizer = Player.GetComponentInChildren<AttackPathVisualizer>();
        if (m_attackPathVisualizer == null) return;

        m_attackPathVisualizer.SetMeshVisible(true);

        m_attackPathVisualizer.useCircle = true;
        m_attackPathVisualizer.innerRadius = 0f;
        m_attackPathVisualizer.outerRadius = m_holdStartRadius;
        m_attackPathVisualizer.angle = 360;
    }

    public override void OnHoldUpdate()
    {
        base.OnHoldUpdate();

        if (m_attackPathVisualizer == null) return;

        m_attackPathVisualizer.outerRadius = math.lerp(
            m_holdStartRadius,
            m_holdFinishRadius,
            GetHoldPercentage());
    }

    public override void OnHoldCancel()
    {
        base.OnHoldCancel();

        if (m_attackPathVisualizer == null) return;
        m_attackPathVisualizer.SetMeshVisible(false);
    }

    public override void OnHoldFinish()
    {
        base.OnHoldFinish();

        if (m_attackPathVisualizer == null) return;
        m_attackPathVisualizer.SetMeshVisible(false);
    }

    public override void OnFinish()
    {
        PlayAnimation("CleaveDefault");

        if (m_attackPathVisualizer == null) return;
        m_attackPathVisualizer.SetMeshVisible(false);
    }
}
