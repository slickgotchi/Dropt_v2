using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class CleaveWhirlwind : PlayerAbility
{
    [Header("CleaveWhirlwind Parameters")]
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
        AbilityRotation = Quaternion.identity;

        // set transform to activation rotation/position and scale based on hold duration
        transform.rotation = AbilityRotation;
        transform.position = PlayerActivationState.position + AbilityOffset;
        float newScale = math.min(1 + (HoldDuration / HoldChargeTime), 2f);
        transform.localScale = new Vector3(newScale, newScale, 1);

        m_hitCounter = NumberHits;
        m_hitTimer = m_hitInterval;

        CollisionCheck();
        m_hitCounter--;

        if (IsClient)
        {
            if (Player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                Animator.Play("CleaveWhirlwind");
                PlayAnimRemoteServerRpc("CleaveWhirlwind", AbilityOffset, AbilityRotation, newScale);
                DebugDraw.DrawColliderPolygon(m_collider, IsServer);
            }

            if (Player.HasComponent<PlayerGotchi>())
            {
                Player.GetComponent<PlayerGotchi>().PlayFacingSpin(3, AbilityDuration / 3,
                    PlayerGotchi.SpinDirection.AntiClockwise, 0);
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

    private void CollisionCheck()
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.Overlap(GetContactFilter("EnemyHurt"), enemyHitColliders);
        bool isLocalPlayer = Player.GetComponent<NetworkObject>().IsLocalPlayer;
        var playerCharacter = Player.GetComponent<NetworkCharacter>();
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var damage = playerCharacter.GetAttackPower() * DamageMultiplier;
                var isCritical = playerCharacter.IsCriticalAttack();
                damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical);
            }
        }

        if (isLocalPlayer && enemyHitColliders.Count > 0)
        {
            Player.GetComponent<PlayerCamera>().Shake(1.5f, 0.3f);
        }

        enemyHitColliders.Clear();
    }


    public override void OnUpdate()
    {
        TrackPlayerPosition();

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

        if (IsClient && Player != null && Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            Animator.Play("CleaveDefault");
            PlayAnimRemoteServerRpc("CleaveDefault", AbilityOffset, AbilityRotation);
        }
    }
}
