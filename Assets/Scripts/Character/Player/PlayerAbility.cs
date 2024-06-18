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

    public NetworkVariable<int> PlayerNetworkObjectId = new NetworkVariable<int>(-1);

    private float m_timer = 0;
    private bool m_isFinished = false;
    
    public bool CanActivate(GameObject playerObject, Hand hand)
    {
        // AP check
        if (playerObject.GetComponent<NetworkCharacter>().ApCurrent.Value < ApCost) return false;

        // Cooldown check
        var playerAbilities = playerObject.GetComponent<PlayerAbilities>();
        if (hand == Hand.Left)
        {
            if (playerAbilities.leftAttackCooldown.Value > 0) return false;
        } else
        {
            if (playerAbilities.rightAttackCooldown.Value > 0) return false;
        }

        // all good!
        return true;
    }

    public bool Activate(GameObject playerObject, StatePayload state, InputPayload input)
    {
        Player = playerObject;
        PlayerActivationState = state;
        PlayerActivationInput = input;

        m_timer = AbilityDuration;
        m_isFinished = false;

        IsActivated = true;
        OnStart();

        return true;
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (!m_isFinished && m_timer < 0)
        {
            OnFinish();
            IsActivated = false;
            m_isFinished = true;
        }
    }

    public virtual void OnStart()
    {

    }

    public virtual void OnFinish()
    {

    }

    protected ContactFilter2D GetEnemyHurtContactFilter()
    {
        return new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << LayerMask.NameToLayer("EnemyHurt"),
            useTriggers = true,
        };
    }

    protected void SetRotationToDirection(Vector3 direction)
    {
        float angle = math.atan2(direction.y, direction.x) * math.TODEGREES;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected Quaternion GetRotationFromDirection(Vector3 direction)
    {
        float angle = math.atan2(direction.y, direction.x) * math.TODEGREES;
        return Quaternion.Euler(0, 0, angle);
    }

    protected void SetPositionToPlayerCenterAtActivation(Vector3 offset = default)
    {
        if (!IsActivated) return;

        transform.position = PlayerActivationState.position + PlayerCenterOffset + offset;
    }

    protected Vector3 GetPlayerCentrePosition()
    {
        Vector3 pos = Vector3.zero;

        if (IsServer && Player != null)
        {
            pos = Player.GetComponent<PlayerMovementAndDash>().GetServerPosition() + PlayerCenterOffset;
        }
        else if (IsClient && Player != null)
        {
            pos = Player.transform.position + PlayerCenterOffset;
        }

        return pos;
    }

    protected void SetPositionToPlayerCenter(Vector3 offset = default)
    {
        if (Player == null || !IsActivated) return;

        transform.position = Player.transform.position + PlayerCenterOffset + offset;
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