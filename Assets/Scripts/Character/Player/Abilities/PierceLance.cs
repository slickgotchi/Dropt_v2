using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class PierceLance : PlayerAbility
{
    [Header("PierceLance Parameters")]
    [SerializeField] float Projection = 0f;
    public float HitRadius = 3.5f;

    // NEW: variables from pierce drill
    [SerializeField] private int NumberHits = 3;
    [SerializeField] private float m_holdStartDistance = 3f;
    [SerializeField] private float m_holdFinishDistance = 14f;

    private Collider2D m_collider;

    private List<Transform> m_hitTransforms = new List<Transform>();

    private float m_speed = 1;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //Debug.Log("On NETWORK SPAWN -> PierceLance");
        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();
    }

    /*
    public override void OnStart()
    {
        // hide the player
        Player.GetComponent<PlayerGotchi>().SetVisible(false);

        var scale = HitRadius / 3.5f;   /// 3.5f is the base size of the lance animation
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    */

    public override void OnStart()
    {
        if (Player == null) return;

        // set transform to activation rotation/position
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        PlayAnimation("PierceDrill");

        m_speed = AutoMoveDistance / AutoMoveDuration;

        PlayerGotchi playerGotchi = Player.GetComponent<PlayerGotchi>();
        playerGotchi.PlayFacingSpin(2, AutoMoveDuration / 2,
            PlayerGotchi.SpinDirection.AntiClockwise, 0);

        playerGotchi.SetGotchiRotation(
            GetAngleFromDirection(ActivationInput.actionDirection) - 90, AutoMoveDuration);

        Player.GetComponent<PlayerController>().StartInvulnerability(ExecutionDuration);

        m_hitTransforms.Clear();

        m_collider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (m_hitTransforms.Contains(collider.gameObject.transform)) return;

        m_hitTransforms.Add(collider.gameObject.transform);

        var networkCharacter = collider.gameObject.GetComponent<NetworkCharacter>();
        if (networkCharacter)
        {
            var playerCharacter = Player.GetComponent<NetworkCharacter>();
            var damage = playerCharacter.currentStaticStats.AttackPower * DamageMultiplier * ActivationWearable.RarityMultiplier;
            damage = GetRandomVariation(damage);
            var isCritical = IsCriticalAttack(playerCharacter.currentStaticStats.CriticalChance);
            damage = (int)(isCritical ? damage * playerCharacter.currentStaticStats.CriticalDamage : damage);
            networkCharacter.TakeDamage(damage, isCritical, Player);

            // do knockback if enemy
            var enemyAI = collider.gameObject.GetComponent<Dropt.EnemyAI>();
            if (enemyAI != null)
            {
                var knockbackDir = Dropt.Utils.Battle.GetVectorFromAtoBAttackCentres(playerCharacter.gameObject, collider.gameObject).normalized;
                enemyAI.Knockback(knockbackDir, KnockbackDistance, KnockbackStunDuration);
            }
        }

        var destructible = collider.gameObject.GetComponent<Destructible>();
        if (destructible != null)
        {
            m_hitTransforms.Add(collider.gameObject.transform);
            destructible.TakeDamage(Wearable.WeaponTypeEnum.Pierce, Player.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

/*
    public override void OnTeleport()
    {
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);

        PlayAnimationWithDuration("PierceLance", ExecutionDuration);

        Player.GetComponent<PlayerGotchi>().SetVisible(true);
        Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchi_PierceLance");
    }

    public override void OnUpdate()
    {
    }

    public override void OnFinish()
    {
        OneFrameCollisionDamageCheck(m_collider, Wearable.WeaponTypeEnum.Pierce, DamageMultiplier);
    }
    */
}
