using Dropt;
using Nethereum.RPC.Shh.KeyPair;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;

// Notes when deriving from PlayerAbility
// - all derived abilities are parented to the player PlayerAbilityCentre by default
//   to change this, in the child OnStart(), call transform.SetParent(null, true).
// 

public class PlayerAbility : NetworkBehaviour
{
    public enum AbilityType { Base, Hold, Special }

    //[Header("Base Ability Parameters")]

    [Tooltip("Set the type of ability")]
    public AbilityType abilityType = AbilityType.Base;

    //[Tooltip("Set to true if this ability should use Special AP Cost from wearable-data spreadsheet")]
    //public bool IsSpecialAbility = false;

    [Tooltip("Cost to cast this ability in AP")]
    public int ApCost = 0;

    [Tooltip("The specific damage multiplier for this ability")]
    public float DamageMultiplier = 1f;

    [Tooltip("Time (s) for the ability to run from Start() to Finish()")]
    public float ExecutionDuration = 1;

    [Tooltip("Slows player down for the AbilityDuration")]
    public float ExecutionSlowFactor = 1;

    [Tooltip("Time (s) taken till any ability can be used after AbilityDuration is Finish()ed")]
    public float CooldownDuration = 0;

    [Tooltip("Slows player down during Cooldown")]
    public float CooldownSlowFactor = 1;

    [Tooltip("Instant teleportation distance in the action direction at ability activation")]
    public float TeleportDistance = 0;

    [Tooltip("Automatically move player over the given distance in the action direction at ability activation (Overrides SlowFactor)")]
    public float AutoMoveDistance = 0;

    [Tooltip("Time taken to move the AutoMoveDistance. Hard capped to AbilityDuration")]
    public float AutoMoveDuration = 0;

    //[Tooltip("Is this ability a hold ability?")]
    //public bool isHoldAbility = false;

    [Tooltip("Duration it takes to charge hold ability")]
    public float HoldChargeTime = 3f;

    [Tooltip("Slows player down during Hold period")]
    public float HoldSlowFactor = 1;

    public float KnockbackDistance = 0f;
    public float KnockbackStunDuration = 0f;

    [HideInInspector] public GameObject Player;
    [HideInInspector] public float SpecialCooldown;

    [Header("Ability Activation Audio")]
    public AudioClip audioOnActivate;

    public Vector3 PlayerAbilityCentreOffset = new Vector3(0, 0.5f, 0);
    protected bool IsActivated = false;
    protected StatePayload PlayerActivationState;
    protected InputPayload ActivationInput;
    protected Wearable.NameEnum ActivationWearableNameEnum;
    protected Wearable ActivationWearable;

    protected float m_holdTimer = 0;

    protected Animator Animator;

    private float m_timer = 0;
    private bool m_isFinished = false;

    private float m_autoMoveTimer = 0;
    private bool m_autoMoveFinishCalled = false;

    private float m_teleportLagTimer = 0;
    private bool m_isOnTeleportStartChecking = false;

    private Transform m_handAndWearableTransform;
    private float m_attackAngleOffset = 0;

    [HideInInspector]
    public enum NetworkRole { LocalClient, RemoteClient, Server }

    //public virtual void OnActivate() { }

    public virtual void OnHoldStart() { }
    public virtual void OnHoldUpdate() { }
    public virtual void OnHoldCancel() { }
    public virtual void OnHoldFinish() { }

    private void Awake()
    {
        AutoMoveDuration = math.min(AutoMoveDuration, ExecutionDuration);
        Animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


    }

    protected Hand AbilityHand;

    public void Init(GameObject playerObject, Hand abilityHand)
    {
        Player = playerObject;
        AbilityHand = abilityHand;
        var playerEquipment = playerObject.GetComponent<PlayerEquipment>();
        var wearableNameEnum = (abilityHand == Hand.Left ? playerEquipment.LeftHand : playerEquipment.RightHand).Value;
        ActivationWearable = WearableManager.Instance.GetWearable(wearableNameEnum);
        SpecialCooldown = ActivationWearable.SpecialCooldown;
        ActivationWearableNameEnum = wearableNameEnum;

        // change wearable hand
        m_handAndWearableTransform = transform.Find("HandAndWearable");
        if (m_handAndWearableTransform == null) return;
        var wearableTransform = m_handAndWearableTransform.Find("Wearable");
        if (wearableTransform == null) return;
        wearableTransform.localPosition = WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, ActivationWearable.AttackView);
        var spriteRenderer = wearableTransform.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        spriteRenderer.sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, ActivationWearable.AttackView);
        m_attackAngleOffset = ActivationWearable.AttackAngle;

        // change secondary slash color
        var secSlashTransform = transform.Find("SecondarySlash");
        if (secSlashTransform == null) return;
        secSlashTransform.GetComponent<SpriteRenderer>().color = ActivationWearable.RarityColor;

        if (IsServer && !IsHost)
        {
            InitClientRpc(playerObject.GetComponent<NetworkObject>().NetworkObjectId, abilityHand);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void InitClientRpc(ulong playerNetworkObjectId, Hand abilityHand)
    {
        var playerObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
        Init(playerObject, abilityHand);
    }

    private bool m_isHoldReady = true;
    private bool m_isHolding = false;

    public void HoldStart()
    {
        if (!m_isHoldReady) return;

        m_isHolding = true;
        m_holdTimer = 0;
        OnHoldStart();
        m_isHoldReady = false;

    }

    public void HoldCancel()
    {
        m_isHolding = false;
        m_holdTimer = 0;
        OnHoldCancel();
        m_isHoldReady = true;
    }

    public void HoldFinish()
    {
        m_isHolding = false;
        m_holdTimer = math.min(m_holdTimer, HoldChargeTime);
        OnHoldFinish();
        m_isHoldReady = true;
    }

    public float GetHoldPercentage()
    {
        var timer = math.min(m_holdTimer, HoldChargeTime);
        return timer / HoldChargeTime;
    }

    private float m_cooldownExpiryTick = 0;

    public bool IsCooldownFinished()
    {
        return m_cooldownExpiryTick < NetworkTimer_v2.Instance.TickCurrent;
    }

    public float GetCooldownRemaining()
    {
        var remainingTicks = m_cooldownExpiryTick - NetworkTimer_v2.Instance.TickCurrent;
        if (remainingTicks <= 0) return 0;

        return remainingTicks * NetworkTimer_v2.Instance.TickInterval;
    }

    public bool Activate(GameObject playerObject, StatePayload state, InputPayload input, float holdDuration)
    {
        //OnActivate();

        Player = playerObject;
        PlayerActivationState = state;
        ActivationInput = input;

        m_holdTimer = math.min(m_holdTimer, HoldChargeTime);

        IsActivated = true;
        m_timer = ExecutionDuration;
        m_isFinished = false;
        m_autoMoveTimer = AutoMoveDuration;
        m_autoMoveFinishCalled = false;
        m_teleportLagTimer = 1 / playerObject.GetComponent<PlayerPrediction>().GetServerTickRate() * 2;
        m_isOnTeleportStartChecking = true;

        // calc cooldown
        var totalCooldownTicks = (int)((ExecutionDuration + CooldownDuration) * NetworkTimer_v2.Instance.TickRate);
        m_cooldownExpiryTick = NetworkTimer_v2.Instance.TickCurrent + totalCooldownTicks;

        // deduct ap from the player
        if (IsServer)
        {
            Player.GetComponent<NetworkCharacter>().ApCurrent.Value -= ApCost;
        }

        // hide the player relevant hand
        if (input.triggeredAbilityEnum != PlayerAbilityEnum.Dash &&
            input.triggeredAbilityEnum != PlayerAbilityEnum.Consume)
        {
            Player.GetComponent<PlayerGotchi>().HideHand(input.abilityHand, ExecutionDuration);
        }

        if (IsClient && audioOnActivate != null)
        {
            AudioManager.Instance.PlaySpatialSFX(audioOnActivate, Vector3.zero, true);
        }

        if (Player != null)
        {
            OnStart();
        }
        else
        {
            Debug.LogWarning("Player = null when calling ability Activate");
        }

        //GameAudioManager.Instance.PlayerAbility(Player.GetComponent<NetworkCharacter>().NetworkObjectId, input.triggeredAbilityEnum, transform.position);
        return true;
    }

    private void LateUpdate()
    {
        // apply attack angle offset to hand and wearable transform
        if (m_handAndWearableTransform != null)
        {
            Quaternion currentRotation = m_handAndWearableTransform.localRotation;
            Quaternion additionalRotation = Quaternion.Euler(0, 0, m_attackAngleOffset);
            m_handAndWearableTransform.localRotation = currentRotation * additionalRotation;
        }

        OnLateUpdate();
    }

    // DO NOT override this in children without calling teh base function, use OnUpdate instead
    protected virtual void Update()
    {
        m_timer -= Time.deltaTime;
        m_autoMoveTimer -= Time.deltaTime;

        if (m_isHolding) OnHoldUpdate();

        if (m_isHolding)
        {
            m_holdTimer += Time.deltaTime;
        }

        if (Player == null) return;

        if (m_autoMoveTimer < 0 && !m_autoMoveFinishCalled && IsActivated)
        {
            OnAutoMoveFinish();
            m_autoMoveFinishCalled = true;
        }

        if (!m_isFinished && m_timer < 0)
        {
            OnFinish();
            IsActivated = false;
            m_isFinished = true;
        }

        if (!m_isFinished) OnUpdate();


        m_teleportLagTimer -= Time.deltaTime;
        if (m_isOnTeleportStartChecking && m_teleportLagTimer < 0)
        {
            OnTeleport();
            m_isOnTeleportStartChecking = false;
        }
    }

    public virtual void OnStart() { }

    public virtual void OnUpdate() { }

    public virtual void OnLateUpdate() { }

    public virtual void OnFinish() { }

    public virtual void OnTeleport() { }

    public virtual void OnAutoMoveFinish() { }

    /// <summary>
    /// Automatically sets the ability rotation to align with the input action direction. Calls SetRotation()
    /// </summary>
    protected void SetRotationToActionDirection()
    {
        SetRotation(GetRotationFromDirection(ActivationInput.actionDirection));
    }

    /// <summary>
    /// Sets ability rotation and then calls RPC's in the background to ensure remote clients rotation instances
    /// are also adjusted
    /// </summary>
    /// <param name="rotation"></param>
    protected void SetRotation(Quaternion rotation)
    {
        // Local Client or Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            transform.rotation = rotation;
        }

        // Server
        if (IsServer)
        {
            SetRotationClientRpc(rotation);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetRotationClientRpc(Quaternion rotation)
    {
        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            transform.rotation = rotation;
        }
    }

    /// <summary>
    /// Sets transform.localPosition. Abilities are parented to their player by default and SetLocalPositon() should generally
    /// be used to configure ability position. For non-parented abilities, spawn different NetworkObjects.
    /// Call RPC's in background to ensure localPosition synced on remote clients.
    /// </summary>
    /// <param name="localPosition"></param>
    protected void SetLocalPosition(Vector3 localPosition)
    {
        // Local Client or Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            transform.localPosition = localPosition;
        }

        // Server
        if (IsServer)
        {
            SetLocalPositionClientRpc(localPosition);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetLocalPositionClientRpc(Vector3 localPosition)
    {
        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            transform.localPosition = localPosition;
        }
    }

    /// <summary>
    /// Set scale for ability. This function also calls RPC's to ensure remote clients ability scales are adjusted.
    /// </summary>
    /// <param name="scale"></param>
    protected void SetScale(float scale)
    {
        // Local Client or Server
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            transform.localScale = new Vector3(scale, scale, 1);
        }

        // Server
        if (IsServer)
        {
            SetScaleClientRpc(scale);
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SetScaleClientRpc(float scale)
    {
        // Remote Client
        if (!Player.GetComponent<NetworkObject>().IsLocalPlayer)
        {
            transform.localScale = new Vector3(scale, scale, 1);
        }
    }

    /// <summary>
    /// Play animation on local instance and on remote clients via RPC. PlayerAbility prefabs should have an Animator
    /// component if they want to use this function.
    /// </summary>
    /// <param name="animName"></param>
    protected void PlayAnimation(string animName, float speed = 1f)
    {
        // Local Client or Server - play animation
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            Animator.speed = speed;
            Animator.Play(animName);
        }

        // Server - send message to all clients to play anim
        if (IsServer)
        {
            PlayAnimationClientRpc(animName, speed);
        }
    }

    protected void PlayAnimationWithDuration(string animName, float duration)
    {
        if (Player == null) return;

        // local player
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer || IsServer)
        {
            Dropt.Utils.Anim.PlayAnimationWithDuration(Animator, animName, duration);
        }

        if (IsServer)
        {
            PlayAnimationWithDurationClientRpc(animName, duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayAnimationWithDurationClientRpc(string animName, float duration)
    {
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer) return;

        Dropt.Utils.Anim.PlayAnimationWithDuration(Animator, animName, duration);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayAnimationClientRpc(string animName, float speed)
    {
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer) return;

        Animator.speed = speed;
        Animator.Play(animName);
    }

    public static ContactFilter2D GetContactFilter(string layerName)
    {
        return new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << LayerMask.NameToLayer(layerName),
            useTriggers = true,
        };
    }

    public static ContactFilter2D GetContactFilter(string[] layerNames)
    {
        int combinedLayerMask = 0;
        foreach (string layerName in layerNames)
        {
            combinedLayerMask |= 1 << LayerMask.NameToLayer(layerName);
        }

        return new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = combinedLayerMask,
            useTriggers = true,
        };
    }

    protected void OneFrameCollisionDamageCheck(Collider2D abilityCollider, Wearable.WeaponTypeEnum weaponType, float damageMultiplier = 1f)
    {
        if (IsServer && !IsHost) PlayerAbility.RollbackEnemies(Player);

        Physics2D.SyncTransforms();
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        abilityCollider.OverlapCollider(GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        bool isLocalPlayer = Player.GetComponent<NetworkObject>().IsLocalPlayer;
        bool isCritical = false;

        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var playerCharacter = Player.GetComponent<NetworkCharacter>();

                if (ActivationWearable != null)
                {
                    var damage = playerCharacter.GetAttackPower() * damageMultiplier * ActivationWearable.RarityMultiplier;
                    isCritical = playerCharacter.IsCriticalAttack();
                    damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);
                    hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Player);

                    var enemyAI = hit.GetComponent<Dropt.EnemyAI>();
                    if (enemyAI != null)
                    {
                        var knockbackDir = Dropt.Utils.Battle.GetVectorFromAtoBAttackCentres(playerCharacter.gameObject, hit.gameObject).normalized;
                        enemyAI.Knockback(knockbackDir, KnockbackDistance, KnockbackStunDuration);
                    }
                }
            }

            if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(weaponType, Player.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
        // screen shake
        if (isLocalPlayer && enemyHitColliders.Count > 0)
        {
            Player.GetComponent<PlayerCamera>().Shake(isCritical ? 1.5f : 0.75f, 0.3f);
        }

        // clear out colliders
        enemyHitColliders.Clear();


        if (IsServer && !IsHost) PlayerAbility.UnrollEnemies();
    }

    public static void RollbackEnemies(GameObject Player)
    {
        // 0. determine lag based on our player
        var playerPing = Player.GetComponent<PlayerPing>();
        if (playerPing == null)
        {
            Debug.Log("No valid player still alive for projectile");
            return;
        }

        // get round trip time
        var rtt_s = (float)playerPing.RTT.Value / 1000;

        // IMPORTANT: There was ALOT of finessing that went into this delay calc and
        // it MIGHT only work with ticks at 15 ticks per second.
        var delay_s = 1f * rtt_s + 0.29f;

        // convert delay in seconds to delay in ticks
        var delay_ticks = delay_s * NetworkTimer_v2.Instance.TickRate;

        // grap the current tick + fraction
        var currentTickAndFraction = NetworkTimer_v2.Instance.TickCurrent + NetworkTimer_v2.Instance.TickFraction;

        // calc the target tick + fraction
        var targetTickAndFraction = currentTickAndFraction - delay_ticks;

        // 1. if we are on server we need to do lag compensation
        var positionBuffers = FindObjectsByType<PositionBuffer>(FindObjectsSortMode.None);
        foreach (var positionBuffer in positionBuffers)
        {
            // stash our enemies current position
            positionBuffer.StashCurrentPosition();

            // calc the new delay position
            var delayPos = positionBuffer.GetPositionAtTickAndFraction(targetTickAndFraction);

            // update to position lagTicks ago
            positionBuffer.transform.position = delayPos;
        }
    }

    public static void UnrollEnemies()
    {
        // reset positions to those that were stashed
        var positionBuffers = FindObjectsByType<PositionBuffer>(FindObjectsSortMode.None);
        foreach (var positionBuffer in positionBuffers)
        {
            // set position back to the stashed position
            positionBuffer.transform.position = positionBuffer.GetStashPosition();
        }
    }

    Vector3 GetAttackVectorFromAToB(GameObject a, GameObject b)
    {
        var aCentre = a.GetComponent<AttackCentre>();
        var aCentrePos = aCentre == null ? a.transform.position : aCentre.transform.position;

        var bCentre = b.GetComponent<AttackCentre>();
        var bCentrePos = bCentre == null ? b.transform.position : bCentre.transform.position;

        return (bCentrePos - aCentrePos).normalized;
    }

    public static Quaternion GetRotationFromDirection(Vector3 direction)
    {
        float angle = PlayerAbility.GetAngleFromDirection(direction);
        return Quaternion.Euler(0, 0, angle);
    }

    public static float GetAngleFromDirection(Vector3 direction)
    {
        return math.atan2(direction.y, direction.x) * math.TODEGREES;
    }

    public static Vector3 GetDirectionFromAngle(float angleInDegrees)
    {
        // Convert angle from degrees to radians
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        // Calculate the x and y components of the vector
        float x = Mathf.Cos(angleInRadians);
        float y = Mathf.Sin(angleInRadians);

        // Create and return the vector
        return new Vector3(x, y, 0f);
    }

    protected Vector3 GetPlayerAbilityCentrePosition()
    {
        Vector3 pos = Vector3.zero;

        if (IsServer && !IsHost && Player != null)
        {
            pos = Player.GetComponent<PlayerPrediction>().GetServerPosition() + PlayerAbilityCentreOffset;
        }
        else if (IsClient && Player != null)
        {
            pos = Player.transform.position + PlayerAbilityCentreOffset;
        }

        return pos;
    }

    /// <summary>
    /// Use this to get some random variation +/- either side of a given base value. Default is 10% either side.
    /// </summary>
    /// <param name="baseValue"></param>
    /// <param name="randomVariation"></param>
    /// <returns></returns>
    public static int GetRandomVariation(float baseValue, float randomVariation = 0.1f)
    {
        return (int)UnityEngine.Random.Range(
            baseValue * (1 - randomVariation),
            baseValue * (1 + randomVariation));
    }

    /// <summary>
    /// Randomly creates a critical attack boolean based on a given critical chance
    /// </summary>
    /// <param name="criticalChance"></param>
    /// <returns></returns>
    public static bool IsCriticalAttack(float criticalChance)
    {
        var rand = UnityEngine.Random.Range(0f, 0.999f);
        return rand < criticalChance;
    }
}

