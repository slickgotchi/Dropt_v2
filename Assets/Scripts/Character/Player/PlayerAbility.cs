using Dropt;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbility : NetworkBehaviour
{
    public int ApCost = 0;
    public float CooldownDuration = 1;
    public float SlowFactor = 1;
    public float SlowDuration = 1;
    public float TeleportDistance = 0;


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