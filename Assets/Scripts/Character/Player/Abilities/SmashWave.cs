using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class SmashWave : PlayerAbility
{
    [Header("SmashWave Parameters")]
    [SerializeField] float Projection = 1f;
    [SerializeField] float m_holdStartDamageMultiplier = 0.5f;
    [SerializeField] float m_holdFinishDamageMultiplier = 2.5f;

    private Collider2D m_collider;
    private List<Collider2D> m_hitColliders = new List<Collider2D>();

    float m_damageMultiplier = 1f;

    public override void OnNetworkSpawn()
    {
        m_collider = GetComponentInChildren<Collider2D>();
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        PlayAnimation("SmashWave");

        m_hitColliders.Clear();

        var alpha = math.min(m_holdTimer / HoldChargeTime, 1f);
        m_damageMultiplier = math.lerp(m_holdStartDamageMultiplier, m_holdFinishDamageMultiplier, alpha);
    }

    public override void OnUpdate()
    {
        CustomCollisionCheck();
    }

    public override void OnFinish()
    {
    }

    public override void OnHoldStart()
    {
    }

    public override void OnHoldFinish()
    {
    }

    private void CustomCollisionCheck()
    {
        // each frame do our collision checks
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {

            bool isAlreadyHit = false;
            foreach (var hitCheck in m_hitColliders)
            {
                if (hitCheck == hit) isAlreadyHit = true;
            }
            if (!isAlreadyHit)
            {
                m_hitColliders.Add(hit);

                if (hit.HasComponent<NetworkCharacter>())
                {
                    var playerCharacter = Player.GetComponent<NetworkCharacter>();
                    var damage = playerCharacter.AttackPower.Value * m_damageMultiplier * ActivationWearable.RarityMultiplier;
                    damage = GetRandomVariation(damage);
                    var isCritical = IsCriticalAttack(playerCharacter.CriticalChance.Value);
                    damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);
                    hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Player);

                    // do knockback if enemy
                    var enemyAI = hit.GetComponent<Dropt.EnemyAI>();
                    if (enemyAI != null)
                    {
                        var knockbackDir = Dropt.Utils.Battle.GetVectorFromAtoBAttackCentres(playerCharacter.gameObject, hit.gameObject).normalized;
                        enemyAI.Knockback(knockbackDir, KnockbackDistance, KnockbackStunDuration);
                    }
                }

                if (hit.HasComponent<Destructible>())
                {
                    var destructible = hit.GetComponent<Destructible>();
                    destructible.TakeDamage(Wearable.WeaponTypeEnum.Smash);
                }
            }
        }
        // clear out colliders
        enemyHitColliders.Clear();
    }
}
