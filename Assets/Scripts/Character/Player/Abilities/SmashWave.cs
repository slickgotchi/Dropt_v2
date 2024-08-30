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

        var alpha = math.min(HoldDuration / HoldChargeTime, 1f);
        m_damageMultiplier = math.lerp(m_holdStartDamageMultiplier, m_holdFinishDamageMultiplier, alpha);
    }

    public override void OnUpdate()
    {
        CustomCollisionCheck();
    }

    public override void OnFinish()
    {

    }

    private void CustomCollisionCheck()
    {
        // each frame do our collision checks
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.Overlap(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
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
