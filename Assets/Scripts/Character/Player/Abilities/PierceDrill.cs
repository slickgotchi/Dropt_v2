using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class PierceDrill : PlayerAbility
{
    [Header("PierceDrill Parameters")]
    [SerializeField] float Projection = 0f;
    [SerializeField] private int NumberHits = 3;
    [SerializeField] private float DamageMultiplier = 0.5f;

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        // set transform to activation rotation/position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        PlayAnimation("PierceDrill");

        Player.GetComponent<PlayerGotchi>().PlayFacingSpin(2, AutoMoveDuration / 2,
            PlayerGotchi.SpinDirection.AntiClockwise, 0);

        Player.GetComponent<PlayerGotchi>().SetGotchiRotation(
            GetAngleFromDirection(ActivationInput.actionDirection) - 90, AutoMoveDuration);
    }

    public override void OnUpdate()
    {
    }

    public override void OnAutoMoveFinish()
    {
        // play the default anim
        PlayAnimation("PierceDefault");

        float chargePower = math.min(1 + (HoldDuration / HoldChargeTime), 2f);
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Pierce, chargePower);
    }
}
