using Dropt;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

// Notes when deriving from PlayerAbility
// - all derived abilities are parented to the player PlayerAbilityCentre by default
//   to change this, in the child OnStart(), call transform.SetParent(null, true).
// 

public class PlayerAbility : NetworkBehaviour
{
    [Header("Base Ability Parameters")]

    [Tooltip("Set to true if this ability should use Special AP Cost from wearable-data spreadsheet")]
    public bool IsSpecialAbility = false;

    [Tooltip("Cost to cast this ability in AP")]
    public int ApCost = 0;

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

    [Tooltip("Is this ability a hold ability?")]
    public bool isHoldAbility = false;

    [Tooltip("Slows player down during Hold period")]
    public float HoldSlowFactor = 1;

    [HideInInspector] public GameObject Player;
    [HideInInspector] public float SpecialCooldown;

    protected Vector3 PlayerCenterOffset = new Vector3(0,0.5f,0);
    protected ContactFilter2D EnemyHurtContactFilter;
    protected bool IsActivated = false;
    protected StatePayload PlayerActivationState;
    protected InputPayload PlayerActivationInput;

    protected float HoldDuration = 0;

    protected Vector3 AbilityOffset = Vector3.zero;
    protected Quaternion AbilityRotation = Quaternion.identity;

    protected Animator Animator;

    private float m_timer = 0;
    private bool m_isFinished = false;

    private float m_autoMoveTimer = 0;
    private bool m_autoMoveFinishCalled = false;

    private void Awake()
    {
        AutoMoveDuration = math.min(AutoMoveDuration, ExecutionDuration);
    }

    public void Init(GameObject playerObject, Hand abilityHand)
    {
        var playerEquipment = playerObject.GetComponent<PlayerEquipment>();
        var wearable = abilityHand == Hand.Left ? playerEquipment.LeftHand : playerEquipment.RightHand;
        ApCost = IsSpecialAbility ? WearableManager.Instance.GetWearable(wearable.Value).SpecialAp : ApCost;
        SpecialCooldown = WearableManager.Instance.GetWearable(wearable.Value).SpecialCooldown;
    }

    public bool Activate(GameObject playerObject, StatePayload state, InputPayload input, float holdDuration)
    {
        Player = playerObject;
        PlayerActivationState = state;
        PlayerActivationInput = input;

        HoldDuration = holdDuration;

        IsActivated = true;
        m_timer = ExecutionDuration;
        m_isFinished = false;
        m_autoMoveTimer = AutoMoveDuration;
        m_autoMoveFinishCalled = false;

        // deduct ap from the player
        if (IsServer)
        {
            Player.GetComponent<NetworkCharacter>().ApCurrent.Value -= ApCost;
        }

        // hide the player relevant hand
        Player.GetComponent<PlayerGotchi>().HideHand(input.abilityHand, ExecutionDuration);

        if (Player != null) OnStart();

        return true;
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        m_autoMoveTimer -= Time.deltaTime;

        if (Player == null) return;

        if (!m_isFinished && m_timer < 0)
        {
            OnFinish();
            IsActivated = false;
            m_isFinished = true;
        }
            
        if (!m_isFinished) OnUpdate();

        if (m_autoMoveTimer < 0 && !m_autoMoveFinishCalled)
        {
            OnAutoMoveFinish();
            m_autoMoveFinishCalled = true;
        }
    }

    public virtual void OnStart() { }

    public virtual void OnUpdate() { }

    public virtual void OnFinish() { }

    public virtual void OnAutoMoveFinish() { }

    [Rpc(SendTo.Server)]
    protected void PlayAnimRemoteServerRpc(string animName, Vector3 abilityOffset, Quaternion abilityRotation, float abilityScale = 1f)
    {
        PlayAnimRemoteClientRpc(animName, abilityOffset, abilityRotation, abilityScale);
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected void PlayAnimRemoteClientRpc(string animName, Vector3 abilityOffset, Quaternion abilityRotation, float abilityScale = 1f)
    {
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer) return;

        AbilityOffset = abilityOffset;
        AbilityRotation = abilityRotation;

        transform.position = Player.transform.position + abilityOffset;
        transform.rotation = abilityRotation;
        transform.localScale = new Vector3(abilityScale, abilityScale, 1);
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

    protected void TrackPlayerPosition()
    {
        if (Player == null) return;
        
        transform.position = Player.transform.position + AbilityOffset;
    }

    protected Quaternion GetRotationFromDirection(Vector3 direction)
    {
        float angle = GetAngleFromDirection(direction);
        return Quaternion.Euler(0, 0, angle);
    }

    protected float GetAngleFromDirection(Vector3 direction)
    {
        return math.atan2(direction.y, direction.x) * math.TODEGREES;
    }

    protected Vector3 GetPlayerCentrePosition()
    {
        Vector3 pos = Vector3.zero;

        if (IsServer && !IsHost && Player != null)
        {
            pos = Player.GetComponent<PlayerPrediction>().GetServerPosition() + PlayerCenterOffset;
        }
        else if (IsClient && Player != null)
        {
            pos = Player.transform.position + PlayerCenterOffset;
        }

        return pos;
    }

}

