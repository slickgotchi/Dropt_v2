using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;

public class PierceDrill : PlayerAbility
{
    [Header("PierceDrill Parameters")]
    [SerializeField] float Projection = 0f;
    [SerializeField] private int NumberHits = 3;
    [SerializeField] private float m_holdStartDistance = 3f;
    [SerializeField] private float m_holdFinishDistance = 14f;
    //private float m_targetDistance = 3f;
    private float m_speed = 1f;

    private Collider2D m_collider;

    private AttackPathVisualizer m_attackPathVisualizer;

    private RaycastHit2D[] m_wallHits = new RaycastHit2D[1];
    private RaycastHit2D[] m_objectHits = new RaycastHit2D[10];

    private List<Transform> m_hitTransforms = new List<Transform>();



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Animator = GetComponent<Animator>();
        m_collider = GetComponent<Collider2D>();

    }

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
    }

    public override void OnUpdate()
    {
        HandleRaycastCollisions(Time.deltaTime);
    }

    public override void OnFinish()
    {
        base.OnFinish();

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

        m_attackPathVisualizer.useCircle = false;
        m_attackPathVisualizer.innerStartPoint = 0f;
        m_attackPathVisualizer.outerFinishPoint = 1f;
        m_attackPathVisualizer.width = 2;

        var playerPrediction = Player.GetComponent<PlayerPrediction>();
        if (playerPrediction == null) return;

        m_attackPathVisualizer.forwardDirection = playerPrediction.GetHoldActionDirection();
    }

    public override void OnHoldUpdate()
    {
        base.OnHoldUpdate();

        if (Player == null) return;
        if (m_attackPathVisualizer == null) return;

        m_attackPathVisualizer.outerFinishPoint = math.lerp(
            m_holdStartDistance,
            m_holdFinishDistance,
            GetHoldPercentage());

        var playerPrediction = Player.GetComponent<PlayerPrediction>();
        if (playerPrediction == null) return;

        m_attackPathVisualizer.forwardDirection = playerPrediction.GetHoldActionDirection();

        // update auto move distance
        AutoMoveDistance = m_attackPathVisualizer.outerFinishPoint;


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

    public override void OnAutoMoveFinish()
    {
        // play the default anim
        PlayAnimation("PierceDefault");
    }

    public void HandleRaycastCollisions(float dt)
    {
        if (IsServer && !IsHost) PlayerAbility.RollbackEnemies(Player);

        // 1. sync transoforms
        Physics2D.SyncTransforms();

        // 2. determine how far we can move (check for wall/water collisions)
        Vector2 castDirection = ActivationInput.actionDirection;
        float castDistance = m_speed * dt;
        int hitCount = m_collider.Cast(castDirection,
            PlayerAbility.GetContactFilter(new string[] { "EnvironmentWall", "EnvironmentWater" }),
            m_wallHits, castDistance);

        if (hitCount > 0)
        {
            var rayHit = m_wallHits[0];
            castDistance = rayHit.distance;
        }

        // 3. perform collisions using the new (if applicable) cast distance
        m_collider.Cast(castDirection,
            PlayerAbility.GetContactFilter(new string[] { "EnemyHurt", "Destructible" }),
            m_objectHits, castDistance);

        // 4. iterate over any object  hits
        for (int i = 0; i < m_objectHits.Length; i++)
        {
            var collider = m_objectHits[i].collider;
            if (collider == null) continue;
            var colliderTransform = collider.transform;
            if (colliderTransform == null) continue;

            bool isAlreadyHit = false;
            for (int j = 0; j < m_hitTransforms.Count; j++)
            {
                if (m_hitTransforms[j] == colliderTransform)
                {
                    isAlreadyHit = true;
                    break;
                }
            }

            if (isAlreadyHit) continue;
            m_hitTransforms.Add(colliderTransform);

            var hit = collider;

            if (hit.HasComponent<NetworkCharacter>())
            {
                var playerCharacter = Player.GetComponent<NetworkCharacter>();
                var damage = playerCharacter.currentStaticStats.AttackPower * DamageMultiplier * ActivationWearable.RarityMultiplier;
                damage = GetRandomVariation(damage);
                var isCritical = IsCriticalAttack(playerCharacter.currentStaticStats.CriticalChance);
                damage = (int)(isCritical ? damage * playerCharacter.currentStaticStats.CriticalDamage : damage);
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
                destructible.TakeDamage(Wearable.WeaponTypeEnum.Pierce, Player.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }
}
