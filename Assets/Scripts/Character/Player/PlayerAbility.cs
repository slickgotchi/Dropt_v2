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
    protected Vector3 PlayerDirection;
    protected ContactFilter2D EnemyHurtContactFilter;

    public NetworkVariable<ulong> PlayerNetworkObjectId = new NetworkVariable<ulong>();

    private float m_timer = 0;
    private bool m_isFinished = false;

    public void TryActivate(GameObject playerObject, Hand hand)
    {
        if (playerObject.GetComponent<NetworkCharacter>().ApCurrent.Value < ApCost) return;

        var playerAbilities = playerObject.GetComponent<PlayerAbilities>();
        if (hand == Hand.Left)
        {
            if (playerAbilities.leftAttackCooldown.Value > 0) return;
        } else
        {
            if (playerAbilities.rightAttackCooldown.Value > 0) return;
        }

        Player = playerObject;
        PlayerDirection = Player.GetComponent<PlayerMovementAndDash>().GetFacingDirection().normalized;

        m_timer = AbilityDuration;
        m_isFinished = false;

        OnStart();
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (!m_isFinished && m_timer < 0)
        {
            OnFinish();
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

    protected void RotateToPlayerDirection()
    {
        float angle = math.atan2(PlayerDirection.y, PlayerDirection.x) * math.TODEGREES;
        transform.rotation = Quaternion.Euler(0, 0, angle);
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