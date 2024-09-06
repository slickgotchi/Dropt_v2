using Dropt;
using Nethereum.RPC.Shh.KeyPair;
using System.Collections;
using System.Collections.Generic;
using Audio.Game;
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
    public enum AbilityType { Base, Hold, Special}

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

    [HideInInspector] public GameObject Player;
    [HideInInspector] public float SpecialCooldown;

    public Vector3 PlayerAbilityCentreOffset = new Vector3(0,0.5f,0);
    protected bool IsActivated = false;
    protected StatePayload PlayerActivationState;
    protected InputPayload ActivationInput;
    protected Wearable.NameEnum ActivationWearableNameEnum;
    protected Wearable ActivationWearable;

    protected float HoldDuration = 0;

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

    private void Awake()
    {
        AutoMoveDuration = math.min(AutoMoveDuration, ExecutionDuration);
        Animator = GetComponent<Animator>();
    }

    public void Init(GameObject playerObject, Hand abilityHand)
    {
        var playerEquipment = playerObject.GetComponent<PlayerEquipment>();
        var wearableNameEnum = (abilityHand == Hand.Left ? playerEquipment.LeftHand : playerEquipment.RightHand).Value;
        ActivationWearable = WearableManager.Instance.GetWearable(wearableNameEnum);
        //ApCost = abilityType == AbilityType.Special ? ActivationWearable.SpecialAp : ApCost;
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

    public bool Activate(GameObject playerObject, StatePayload state, InputPayload input, float holdDuration)
    {
        Player = playerObject;
        PlayerActivationState = state;
        ActivationInput = input;

        HoldDuration = holdDuration;

        IsActivated = true;
        m_timer = ExecutionDuration;
        m_isFinished = false;
        m_autoMoveTimer = AutoMoveDuration;
        m_autoMoveFinishCalled = false;
        m_teleportLagTimer = 1 / playerObject.GetComponent<PlayerPrediction>().GetServerTickRate() * 2;
        m_isOnTeleportStartChecking = true;

        // deduct ap from the player
        if (IsServer)
        {
            Player.GetComponent<NetworkCharacter>().ApCurrent.Value -= ApCost;
        }

        // hide the player relevant hand
        if (input.abilityTriggered != PlayerAbilityEnum.Dash &&
            input.abilityTriggered != PlayerAbilityEnum.Consume)
        {
            Player.GetComponent<PlayerGotchi>().HideHand(input.abilityHand, ExecutionDuration);
        }

        if (Player != null) OnStart();

        GameAudioManager.Instance.PlayerAbility(Player.GetComponent<NetworkCharacter>().NetworkObjectId, input.abilityTriggered, transform.position);
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
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;
        m_autoMoveTimer -= Time.deltaTime;

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
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        abilityCollider.OverlapCollider(GetContactFilter(new string[] { "EnemyHurt", "Destructible" }), enemyHitColliders);
        bool isLocalPlayer = Player.GetComponent<NetworkObject>().IsLocalPlayer;
        bool isCritical = false;
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                var playerCharacter = Player.GetComponent<NetworkCharacter>();
                var damage = playerCharacter.GetAttackPower() * damageMultiplier * ActivationWearable.RarityMultiplier;
                isCritical = playerCharacter.IsCriticalAttack();
                damage = (int)(isCritical ? damage * playerCharacter.CriticalDamage.Value : damage);
                hit.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, Player);
            }

            if (hit.HasComponent<Destructible>())
            {
                var destructible = hit.GetComponent<Destructible>();
                destructible.TakeDamage(weaponType);
            }
        }

        // screen shake
        if (isLocalPlayer && enemyHitColliders.Count > 0)
        {
            Player.GetComponent<PlayerCamera>().Shake(isCritical ? 1.5f : 0.75f, 0.3f);
        }

        // clear out colliders
        enemyHitColliders.Clear();
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

