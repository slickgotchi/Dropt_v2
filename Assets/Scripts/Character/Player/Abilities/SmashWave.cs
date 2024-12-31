using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class SmashWave : PlayerAbility
{
    [Header("SmashWave Parameters")]
    [SerializeField] float Projection = 1f;
    [SerializeField] float m_holdStartKnockbackMultiplier = 0.1f;
    [SerializeField] float m_holdFinishKnockbackMultiplier = 1f;
    [SerializeField] private float m_holdStartRadius = 2f;
    [SerializeField] private float m_holdFinishRadius = 7f;

    // these are what i scale
    [Header("GameObjects to scale during attack")]
    [SerializeField] private List<GameObject> scaleObjects = new List<GameObject>();

    private Collider2D m_collider;
    private List<Collider2D> m_hitColliders = new List<Collider2D>();

    float m_damageMultiplier = 1f;
    float m_knockbackMultiplier = 0.1f;

    private AttackPathVisualizer m_attackPathVisualizer;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_collider = GetComponentInChildren<Collider2D>();
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection * Projection);

        // calc new scale, the base animation has radius of 2.5, so scale 1 == radius 2.5
        var targetRadius = math.lerp(m_holdStartRadius, m_holdFinishRadius, GetHoldPercentage());
        SetSmashWaveScale(targetRadius / 2.5f);

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        PlayAnimation("SmashWave");

        m_hitColliders.Clear();

        m_knockbackMultiplier = math.min(m_holdTimer / HoldChargeTime, 1f);
        //CustomCollisionCheck();
        //m_damageMultiplier = math.lerp(m_holdStartKnockbackMultiplier, m_holdFinishKnockbackMultiplier, alpha);

        m_collisionTimer = 0;
        m_isCollided = false;
    }

    protected void SetSmashWaveScale(float scale)
    {
        // Local Client or Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            foreach (var so in scaleObjects)
            {
                so.transform.localScale = new Vector3(scale, scale, 1);
            }
            //transform.localScale = new Vector3(scale, scale, 1);
        }

        // Server
        if (IsServer)
        {
            SetSmashWaveScaleClientRpc(scale);
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SetSmashWaveScaleClientRpc(float scale)
    {
        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            foreach (var so in scaleObjects)
            {
                so.transform.localScale = new Vector3(scale, scale, 1);
            }
        }
    }

    private bool m_isCollided = false;
    private float m_collisionTimer = 0f;

    public override void OnUpdate()
    {
        m_collisionTimer += Time.deltaTime;

        // we check collision once halfway through execution
        if (m_collisionTimer > 0.5f * ExecutionDuration && !m_isCollided)
        {
            CustomCollisionCheck();
            m_isCollided = true;
        }
    }

    public override void OnFinish()
    {
        if (m_attackPathVisualizer == null) return;
        if (!IsHolding()) m_attackPathVisualizer.SetMeshVisible(false);
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
        m_attackPathVisualizer.angle = 90;

        var playerPrediction = Player.GetComponent<PlayerPrediction>();
        if (playerPrediction == null) return;

        m_attackPathVisualizer.forwardDirection = playerPrediction.GetHoldActionDirection();
    }

    public override void OnHoldUpdate()
    {
        base.OnHoldUpdate();

        if (Player == null) return;
        if (m_attackPathVisualizer == null) return;

        m_attackPathVisualizer.outerRadius = math.lerp(
            m_holdStartRadius,
            m_holdFinishRadius,
            GetHoldPercentage());

        var playerPrediction = Player.GetComponent<PlayerPrediction>();
        if (playerPrediction == null) return;

        m_attackPathVisualizer.forwardDirection = playerPrediction.GetHoldActionDirection();
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

    private void CustomCollisionCheck()
    {
        if (IsServer && !IsHost) RollbackEnemies(Player);

        // resync transforms
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.OverlapCollider(GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        foreach (Collider2D hit in enemyHitColliders)
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
                    var damage = playerCharacter.currentStaticStats.AttackPower * m_damageMultiplier * ActivationWearable.RarityMultiplier;
                    damage = GetRandomVariation(damage);
                    var isCritical = IsCriticalAttack(playerCharacter.currentStaticStats.CriticalChance);
                    damage = (int)(isCritical ? damage * playerCharacter.currentStaticStats.CriticalDamage : damage);
                    hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Player);

                    // do knockback if enemy
                    var enemyAI = hit.GetComponent<Dropt.EnemyAI>();
                    if (enemyAI != null)
                    {
                        var knockbackDir = Dropt.Utils.Battle.GetVectorFromAtoBAttackCentres(playerCharacter.gameObject, hit.gameObject).normalized;
                        enemyAI.Knockback(knockbackDir, KnockbackDistance * m_knockbackMultiplier, KnockbackStunDuration);
                    }
                }

                if (hit.HasComponent<Destructible>())
                {
                    var destructible = hit.GetComponent<Destructible>();
                    destructible.TakeDamage(Wearable.WeaponTypeEnum.Smash, Player.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
        }
        // clear out colliders
        enemyHitColliders.Clear();

        if (IsServer && !IsHost) UnrollEnemies();
    }

}