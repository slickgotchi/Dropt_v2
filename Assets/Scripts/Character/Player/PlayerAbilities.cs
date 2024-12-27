using Unity.Netcode;
using UnityEngine;


public class PlayerAbilities : NetworkBehaviour
{
    [Header("Dash")]
    public GameObject Dash;

    [Header("Cleave")]
    public GameObject CleaveSlash;
    public GameObject CleaveWhirlwind;
    public GameObject CleaveCyclone;

    [Header("Pierce")]
    public GameObject PierceThrust;
    public GameObject PierceDrill;
    public GameObject PierceLance;

    [Header("Smash")]
    public GameObject SmashSwipe;
    public GameObject SmashWave;
    public GameObject SmashSlam;

    [Header("Ballistic")]
    public GameObject BallisticShot;
    public GameObject BallisticSnipe;
    public GameObject BallisticExplosion;

    [Header("Magic")]
    public GameObject MagicCast;
    public GameObject MagicBeam;
    public GameObject MagicBlast;

    [Header("Splash")]
    public GameObject SplashLob;
    public GameObject SplashVolley;
    public GameObject SplashBomb;

    [Header("Shield")]
    public GameObject ShieldBash;
    public GameObject shieldBlock;
    public GameObject ShieldWall;

    [Header("Unarmed")]
    public GameObject UnarmedPunch;

    // cooldowns
    [HideInInspector] public NetworkVariable<float> leftAttackCooldown = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> rightAttackCooldown = new NetworkVariable<float>(0);

    private void Start()
    {
        // Ensure we instatiate here in Start() AFTER OnNetworkSpawn() otherwise we end up with duplciates
        if (!IsServer) return;
    }

    private void OnDisable()
    {
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

    }

    public override void OnNetworkDespawn()
    {

        if (Bootstrap.IsUnityEditor())
        {
            // check on application quit action reason
            if (null == NetworkManager.RpcTarget)
                return;
        }

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (IsServer && IsSpawned && leftAttackCooldown != null && rightAttackCooldown != null)
        {
            leftAttackCooldown.Value -= dt;
            rightAttackCooldown.Value -= dt;
        }
    }

    public PlayerAbility GetAbility(PlayerAbilityEnum abilityEnum)
    {
        if (!IsSpawned || abilityEnum == PlayerAbilityEnum.Null) return null;

        // dash
        if (abilityEnum == PlayerAbilityEnum.Dash && Dash != null) return Dash.GetComponent<PlayerAbility>();

        // cleave
        if (abilityEnum == PlayerAbilityEnum.CleaveSlash && CleaveSlash != null) return CleaveSlash.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.CleaveWhirlwind && CleaveWhirlwind != null) return CleaveWhirlwind.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.CleaveCyclone && CleaveCyclone != null) return CleaveCyclone.GetComponent<PlayerAbility>();

        // pierce
        if (abilityEnum == PlayerAbilityEnum.PierceThrust && PierceThrust != null) return PierceThrust.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.PierceDrill && PierceDrill != null) return PierceDrill.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.PierceLance && PierceLance != null) return PierceLance.GetComponent<PlayerAbility>();

        // smash
        if (abilityEnum == PlayerAbilityEnum.SmashSwipe && SmashSwipe != null) return SmashSwipe.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.SmashWave && SmashWave != null) return SmashWave.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.SmashSlam && SmashSlam != null) return SmashSlam.GetComponent<PlayerAbility>();

        // ballistic
        if (abilityEnum == PlayerAbilityEnum.BallisticShot && BallisticShot != null) return BallisticShot.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.BallisticSnipe && BallisticSnipe != null) return BallisticSnipe.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.BallisticKill && BallisticExplosion != null) return BallisticExplosion.GetComponent<PlayerAbility>();

        // magic
        if (abilityEnum == PlayerAbilityEnum.MagicCast && MagicCast != null) return MagicCast.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.MagicBeam && MagicBeam != null) return MagicBeam.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.MagicBlast && MagicBlast != null) return MagicBlast.GetComponent<PlayerAbility>();

        // splash
        if (abilityEnum == PlayerAbilityEnum.SplashLob && SplashLob != null) return SplashLob.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.SplashVolley && SplashVolley != null) return SplashVolley.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.SplashBomb && SplashBomb != null) return SplashBomb.GetComponent<PlayerAbility>();

        // shield
        if (abilityEnum == PlayerAbilityEnum.ShieldBash && ShieldBash != null) return ShieldBash.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.ShieldBlock && shieldBlock != null) return shieldBlock.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.ShieldWall && ShieldWall != null) return ShieldWall.GetComponent<PlayerAbility>();

        // unarmed
        if (abilityEnum == PlayerAbilityEnum.Unarmed && UnarmedPunch != null) return UnarmedPunch.GetComponent<PlayerAbility>();

        return null;
    }

    public PlayerAbilityEnum GetAttackAbilityEnum(Wearable.NameEnum wearableNameEnum)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Cleave) return PlayerAbilityEnum.CleaveSlash;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Smash) return PlayerAbilityEnum.SmashSwipe;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Pierce) return PlayerAbilityEnum.PierceThrust;

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Ballistic) return PlayerAbilityEnum.BallisticShot;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Magic) return PlayerAbilityEnum.MagicCast;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Splash) return PlayerAbilityEnum.SplashLob;

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Consume) return PlayerAbilityEnum.Unarmed;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Aura) return PlayerAbilityEnum.Unarmed;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Throw) return PlayerAbilityEnum.Unarmed;

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Shield) return PlayerAbilityEnum.ShieldBash;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Unarmed) return PlayerAbilityEnum.Unarmed;

        return PlayerAbilityEnum.Null;
    }

    public PlayerAbilityEnum GetHoldAbilityEnum(Wearable.NameEnum wearableNameEnum)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Cleave) return PlayerAbilityEnum.CleaveWhirlwind;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Smash) return PlayerAbilityEnum.SmashWave;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Pierce) return PlayerAbilityEnum.PierceDrill;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Ballistic) return PlayerAbilityEnum.BallisticSnipe;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Magic) return PlayerAbilityEnum.MagicBeam;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Splash) return PlayerAbilityEnum.SplashVolley;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Shield) return PlayerAbilityEnum.ShieldBlock;

        return PlayerAbilityEnum.Null;
    }


    public PlayerAbilityEnum GetSpecialAbilityEnum(Wearable.NameEnum wearableNameEnum)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Cleave) return PlayerAbilityEnum.CleaveCyclone;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Smash) return PlayerAbilityEnum.SmashSlam;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Pierce) return PlayerAbilityEnum.PierceLance;

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Ballistic) return PlayerAbilityEnum.BallisticKill;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Magic) return PlayerAbilityEnum.MagicBlast;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Splash) return PlayerAbilityEnum.SplashBomb;

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Consume) return PlayerAbilityEnum.Consume;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Aura) return PlayerAbilityEnum.Aura;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Throw) return PlayerAbilityEnum.Throw;

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Shield) return PlayerAbilityEnum.ShieldWall;

        return PlayerAbilityEnum.Null;
    }
}



public enum Hand { Left, Right };

public enum PlayerAbilityEnum
{
    Null,
    Dash,
    CleaveSlash, CleaveWhirlwind, CleaveCyclone,
    SmashSwipe, SmashWave, SmashSlam,
    PierceThrust, PierceDrill, PierceLance,
    BallisticShot, BallisticSnipe, BallisticKill,
    MagicCast, MagicBeam, MagicBlast,
    SplashLob, SplashVolley, SplashBomb,
    Consume, Aura, Throw,
    ShieldBash, ShieldBlock, ShieldWall,
    Unarmed,
}