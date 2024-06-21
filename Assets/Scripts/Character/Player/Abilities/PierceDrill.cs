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
    [SerializeField] private float HoldChargeTime = 3f;

    private float m_hitInterval;
    private float m_hitTimer;
    private int m_hitCounter;
    

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();

        m_hitInterval = AbilityDuration / (NumberHits - 1);
    }

    public override void OnStart()
    {
        AbilityOffset = PlayerCenterOffset + PlayerActivationInput.actionDirection * Projection;
        AbilityRotation = GetRotationFromDirection(PlayerActivationInput.actionDirection);

        // set transform to activation rotation/position and scale based on hold duration
        transform.rotation = AbilityRotation;
        transform.position = PlayerActivationState.position + AbilityOffset;
        float chargePower = math.min(1 + (HoldDuration / HoldChargeTime), 2f);

        m_hitCounter = NumberHits;
        m_hitTimer = m_hitInterval;


        if (IsClient)
        {
            if (Player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                Animator.Play("PierceDrill");
                PlayAnimRemoteServerRpc("PierceDrill", AbilityOffset, AbilityRotation);
                DebugDraw.DrawColliderPolygon(m_collider, IsServer);
            }

            if (Player.HasComponent<PlayerGotchi>())
            {
                Player.GetComponent<PlayerGotchi>().PlayFacingSpin(3, AbilityDuration / 3,
                    PlayerGotchi.SpinDirection.AntiClockwise, 0);
                Player.GetComponent<PlayerGotchi>().SetGotchiRotation(
                    GetAngleFromDirection(PlayerActivationInput.actionDirection)-90, AbilityDuration);
                PlayFacingSpinRemoteServerRpc(3, AbilityDuration / 3, PlayerGotchi.SpinDirection.AntiClockwise, 0);
            }
        }
    }

    [Rpc(SendTo.Server)]
    void PlayFacingSpinRemoteServerRpc(int spinNumber, float spinPeriod, PlayerGotchi.SpinDirection spinDirection,
        float startAngle)
    {
        PlayFacingSpinRemoteClientRpc(spinNumber, spinPeriod, spinDirection, startAngle);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PlayFacingSpinRemoteClientRpc(int spinNumber, float spinPeriod, PlayerGotchi.SpinDirection spinDirection,
        float startAngle)
    {
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer) return;

        Player.GetComponent<PlayerGotchi>().PlayFacingSpin(spinNumber, spinPeriod, spinDirection, startAngle);
    }

    public override void OnUpdate()
    {
        TrackPlayerPosition();

    }

    public override void OnFinish()
    {
        // we do one last collision check here

        // play the default anim
        if (IsClient && Player != null && Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            Animator.Play("PierceDefault");
            PlayAnimRemoteServerRpc("PierceDefault", AbilityOffset, AbilityRotation);
        }
    }
}
