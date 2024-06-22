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

    private Collider2D m_collider;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        AbilityOffset = PlayerCenterOffset + PlayerActivationInput.actionDirection * Projection;
        AbilityRotation = GetRotationFromDirection(PlayerActivationInput.actionDirection);

        // set transform to activation rotation/position and scale based on hold duration
        transform.rotation = AbilityRotation;
        transform.position = PlayerActivationState.position + AbilityOffset;

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
                Player.GetComponent<PlayerGotchi>().PlayFacingSpin(2, AutoMoveDuration/2,
                    PlayerGotchi.SpinDirection.AntiClockwise, 0);
                Player.GetComponent<PlayerGotchi>().SetGotchiRotation(
                    GetAngleFromDirection(PlayerActivationInput.actionDirection) - 90, AutoMoveDuration);
                PlayFacingSpinRemoteServerRpc(2, AutoMoveDuration / 2, PlayerGotchi.SpinDirection.AntiClockwise, 0);
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
        //Debug.Log(Player.GetComponent<PlayerGotchi>().GetGotchi().transform.position + " " + transform.position);
        //Debug.Log(Player.transform.position + " " + transform.position);
    }

    public override void OnAutoMoveFinish()
    {
        // play the default anim
        if (IsClient && Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            Animator.Play("PierceDefault");
            PlayAnimRemoteServerRpc("PierceDefault", AbilityOffset, AbilityRotation);
        }

        float chargePower = math.min(1 + (HoldDuration / HoldChargeTime), 2f);

        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.Overlap(GetContactFilter("EnemyHurt"), enemyHitColliders);
        bool isLocalPlayer = Player.GetComponent<NetworkObject>().IsLocalPlayer;
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var playerCharacter = Player.GetComponent<NetworkCharacter>();
                var damage = playerCharacter.GetAttackPower() * chargePower;
                var isCritical = playerCharacter.IsCriticalAttack();
                damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical);
            }
        }

        // screen shake
        if (isLocalPlayer && enemyHitColliders.Count > 0)
        {
            Player.GetComponent<PlayerCamera>().Shake(1.5f, 0.3f);
        }

        // clear out colliders
        enemyHitColliders.Clear();
    }
}
