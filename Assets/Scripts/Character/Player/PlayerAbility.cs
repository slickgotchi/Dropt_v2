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
    public int ApCost = 0;
    public float AbilityDuration = 1;
    public float CooldownDuration = 1;
    public float SlowFactor = 1;
    public float SlowFactorDuration = 1;
    public float TeleportDistance = 0;

    public GameObject Player;
    protected Vector3 PlayerCenterOffset = new Vector3(0,0.5f,0);
    protected ContactFilter2D EnemyHurtContactFilter;
    protected bool IsActivated = false;
    protected StatePayload PlayerActivationState;
    protected InputPayload PlayerActivationInput;

    protected float HoldDuration = 0;

    protected Vector3 AbilityOffset = Vector3.zero;
    protected Quaternion AbilityRotation = Quaternion.identity;

    protected Animator Animator;

    public NetworkVariable<int> PlayerNetworkObjectId = new NetworkVariable<int>(-1);

    private float m_timer = 0;
    private bool m_isFinished = false;
    private bool m_isServer = false;

    public bool CanActivate(GameObject playerObject, Hand hand)
    {
        // AP check
        if (playerObject.GetComponent<NetworkCharacter>().ApCurrent.Value < ApCost) return false;

        // Cooldown check
        var playerAbilities = playerObject.GetComponent<PlayerAbilities>();
        if (hand == Hand.Left)
        {
            if (playerAbilities.leftAttackCooldown.Value > 0) return false;
            if (IsServer) playerAbilities.leftAttackCooldown.Value = CooldownDuration;
        } else
        {
            if (playerAbilities.rightAttackCooldown.Value > 0) return false;
            if (IsServer) playerAbilities.rightAttackCooldown.Value = CooldownDuration;
        }

        // all good!
        return true;
    }

    public bool Activate(GameObject playerObject, StatePayload state, InputPayload input, float holdDuration, bool isServer = false)
    {
        Player = playerObject;
        PlayerActivationState = state;
        PlayerActivationInput = input;

        HoldDuration = holdDuration;

        m_timer = AbilityDuration;
        m_isFinished = false;

        IsActivated = true;
        m_isServer = isServer;
        OnStart(m_isServer);

        return true;
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (!m_isFinished && m_timer < 0)
        {
            OnFinish(m_isServer);
            IsActivated = false;
            m_isFinished = true;
        }
    }

    public virtual void OnStart(bool isServer = false)
    {

    }

    public virtual void OnFinish(bool isServer = false)
    {

    }

    [Rpc(SendTo.Server)]
    protected void PlayAnimRemoteServerRpc(string animName, Vector3 abilityOffset, Quaternion abilityRotation)
    {
        PlayAnimRemoteClientRpc(animName, abilityOffset, abilityRotation);
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected void PlayAnimRemoteClientRpc(string animName, Vector3 abilityOffset, Quaternion abilityRotation)
    {
        if (Player.GetComponent<NetworkObject>().IsLocalPlayer) return;

        AbilityOffset = abilityOffset;
        AbilityRotation = abilityRotation;

        transform.position = Player.transform.position + abilityOffset;
        transform.rotation = abilityRotation;
        Animator.Play(animName);
    }

    protected ContactFilter2D GetContactFilter(string layerName)
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
        float angle = math.atan2(direction.y, direction.x) * math.TODEGREES;
        return Quaternion.Euler(0, 0, angle);
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

public enum PlayerAbilityEnum
{
    Null,
    Dash,
    CleaveSwing, CleaveWhirlwind, CleaveCyclone,
    SmashSwing, SmashWave, SmashSlam,
    PierceThrust, PierceDrill, PierceLance,
    BallisticShot, BallisticSnipe, BallisticKill,
    MagicCast, MagicBeam, MagicBlast,
    SplashLob, SplashVolley, SplashBomb,
    Consume, Aura, Throw,
    ShieldBash, ShieldParry, ShieldWall,
    Unarmed,
}