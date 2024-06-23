using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class CleaveCyclone : PlayerAbility
{
    [Header("CleaveCyclone Parameters")]
    public float Projection = 1.5f;
    public float Distance = 10f;
    public float Duration = 3f;
    public float Scale = 2f;
    public float DamageMultiplierPerHit = 0.5f;
    public int NumberHits = 6;

    public GameObject CleaveCycloneProjectilePrefab;

    private Vector3 m_abilityDirection;

    private Collider2D m_collider;
    private GameObject m_projectile;

    public override void OnNetworkSpawn()
    {
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnStart()
    {
        //AbilityOffset = PlayerCenterOffset;
        AbilityOffset = PlayerCenterOffset + PlayerActivationInput.actionDirection * Projection;
        AbilityRotation = quaternion.identity;

        // set transform to activation rotation/position
        transform.rotation = AbilityRotation;
        transform.position = PlayerActivationState.position + AbilityOffset;

        m_abilityDirection = PlayerActivationInput.actionDirection;

        if (IsServer)
        {
            m_projectile = Instantiate(CleaveCycloneProjectilePrefab, transform);
            var no_projectile = m_projectile.GetComponent<CleaveCycloneProjectile>();
            no_projectile.Direction = PlayerActivationInput.actionDirection;
            no_projectile.Distance = Distance;
            no_projectile.Duration = Duration;
            no_projectile.Scale = Scale;

            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            no_projectile.DamagePerHit = playerCharacter.AttackPower.Value;
            no_projectile.CriticalChance = playerCharacter.CriticalChance.Value;
            no_projectile.CriticalDamage = playerCharacter.CriticalDamage.Value;

            m_projectile.GetComponent<NetworkObject>().Spawn();
        }

        // animation
        bool isLocalPlayer = Player.GetComponent<NetworkObject>().IsLocalPlayer;
        if (isLocalPlayer)
        {
            //Player.GetComponent<PlayerGotchi>().PlayFacingSpin(1, 0.5f, 
            //    PlayerGotchi.SpinDirection.Clockwise, GetAngleFromDirection(PlayerActivationInput.actionDirection));

            //Animator.Play("CleaveCyclone");
            //DebugDraw.DrawColliderPolygon(m_collider, IsServer);
            //PlayAnimRemoteServerRpc("CleaveCyclone", AbilityOffset, AbilityRotation);
        }
    }



    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
    }
}
