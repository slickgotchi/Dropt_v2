using Unity.Netcode;
using UnityEngine;


public class PlayerAbilities : NetworkBehaviour
{
    [Header("Dash")]
    public GameObject dashPrefab;
    [HideInInspector] public GameObject Dash;
    private NetworkVariable<ulong> DashId = new NetworkVariable<ulong>(0);

    [Header("Cleave")]
    public GameObject cleaveSlashPrefab;
    [HideInInspector] public GameObject CleaveSlash;
    private NetworkVariable<ulong> CleaveSlashId = new NetworkVariable<ulong>(0);

    public GameObject cleaveWhirlwindPrefab;
    [HideInInspector] public GameObject CleaveWhirlwind;
    private NetworkVariable<ulong> CleaveWhirlwindId = new NetworkVariable<ulong>(0);

    public GameObject cleaveCyclonePrefab;
    [HideInInspector] public GameObject CleaveCyclone;
    private NetworkVariable<ulong> CleaveCycloneId = new NetworkVariable<ulong>(0);

    [Header("Pierce")]
    public GameObject pierceThrustPrefab;
    [HideInInspector] public GameObject PierceThrust;
    private NetworkVariable<ulong> PierceThrustId = new NetworkVariable<ulong>(0);

    public GameObject pierceDrillPrefab;
    [HideInInspector] public GameObject PierceDrill;
    private NetworkVariable<ulong> PierceDrillId = new NetworkVariable<ulong>(0);

    public GameObject pierceLancePrefab;
    [HideInInspector] public GameObject PierceLance;
    private NetworkVariable<ulong> PierceLanceId = new NetworkVariable<ulong>(0);

    [Header("Smash")]
    public GameObject smashSwipePrefab;
    [HideInInspector] public GameObject SmashSwipe;
    private NetworkVariable<ulong> SmashSwipeId = new NetworkVariable<ulong>(0);

    public GameObject smashWavePrefab;
    [HideInInspector] public GameObject SmashWave;
    private NetworkVariable<ulong> SmashWaveId = new NetworkVariable<ulong>(0);

    public GameObject smashSlamPrefab;
    [HideInInspector] public GameObject SmashSlam;
    private NetworkVariable<ulong> SmashSlamId = new NetworkVariable<ulong>(0);

    [Header("Ballistic")]
    public GameObject ballisticShotPrefab;
    [HideInInspector] public GameObject BallisticShot;
    private NetworkVariable<ulong> BallisticShotId = new NetworkVariable<ulong>(0);

    public GameObject ballisticSnipePrefab;
    [HideInInspector] public GameObject BallisticSnipe;
    private NetworkVariable<ulong> BallisticSnipeId = new NetworkVariable<ulong>(0);

    public GameObject ballisticExplosionPrefab;
    [HideInInspector] public GameObject BallisticExplosion;
    private NetworkVariable<ulong> BallisticExplosionId = new NetworkVariable<ulong>(0);

    [Header("Magic")]
    public GameObject magicCastPrefab;
    [HideInInspector] public GameObject MagicCast;
    private NetworkVariable<ulong> MagicCastId = new NetworkVariable<ulong>(0);

    public GameObject magicBeamPrefab;
    [HideInInspector] public GameObject MagicBeam;
    private NetworkVariable<ulong> MagicBeamId = new NetworkVariable<ulong>(0);

    public GameObject magicBlastPrefab;
    [HideInInspector] public GameObject MagicBlast;
    private NetworkVariable<ulong> MagicBlastId = new NetworkVariable<ulong>(0);

    [Header("Splash")]
    public GameObject splashLobPrefab;
    [HideInInspector] public GameObject SplashLob;
    private NetworkVariable<ulong> SplashLobId = new NetworkVariable<ulong>(0);

    public GameObject splashVolleyPrefab;
    [HideInInspector] public GameObject SplashVolley;
    private NetworkVariable<ulong> SplashVolleyId = new NetworkVariable<ulong>(0);

    public GameObject splashBombPrefab;
    [HideInInspector] public GameObject SplashBomb;
    private NetworkVariable<ulong> SplashBombId = new NetworkVariable<ulong>(0);

    [Header("Shield")]
    public GameObject shieldBashPrefab;
    [HideInInspector] public GameObject ShieldBash;
    private NetworkVariable<ulong> ShieldBashId = new NetworkVariable<ulong>(0);

    public GameObject shieldBlockPrefab;
    [HideInInspector] public GameObject shieldBlock;
    private NetworkVariable<ulong> ShieldBlockId = new NetworkVariable<ulong>(0);

    public GameObject shieldWallPrefab;
    [HideInInspector] public GameObject ShieldWall;
    private NetworkVariable<ulong> ShieldWallId = new NetworkVariable<ulong>(0);

    [Header("Unarmed")]
    public GameObject unarmedPunchPrefab;
    [HideInInspector] public GameObject UnarmedPunch;
    private NetworkVariable<ulong> UnarmedPunchId = new NetworkVariable<ulong>(0);

    // cooldowns
    [HideInInspector] public NetworkVariable<float> leftAttackCooldown = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> rightAttackCooldown = new NetworkVariable<float>(0);

    private void Start()
    {
        // Ensure we instatiate here in Start() AFTER OnNetworkSpawn() otherwise we end up with duplciates
        if (!IsServer) return;

        // dash
        CreateAbility(ref Dash, dashPrefab, DashId);

        // cleave
        CreateAbility(ref CleaveSlash, cleaveSlashPrefab, CleaveSlashId);
        CreateAbility(ref CleaveWhirlwind, cleaveWhirlwindPrefab, CleaveWhirlwindId);
        CreateAbility(ref CleaveCyclone, cleaveCyclonePrefab, CleaveCycloneId);

        // pierce
        CreateAbility(ref PierceThrust, pierceThrustPrefab, PierceThrustId);
        CreateAbility(ref PierceDrill, pierceDrillPrefab, PierceDrillId);
        CreateAbility(ref PierceLance, pierceLancePrefab, PierceLanceId);

        // smash
        CreateAbility(ref SmashSwipe, smashSwipePrefab, SmashSwipeId);
        CreateAbility(ref SmashWave, smashWavePrefab, SmashWaveId);
        CreateAbility(ref SmashSlam, smashSlamPrefab, SmashSlamId);

        // ballistic
        CreateAbility(ref BallisticShot, ballisticShotPrefab, BallisticShotId);
        CreateAbility(ref BallisticSnipe, ballisticSnipePrefab, BallisticSnipeId);
        CreateAbility(ref BallisticExplosion, ballisticExplosionPrefab, BallisticExplosionId);

        // magic
        CreateAbility(ref MagicCast, magicCastPrefab, MagicCastId);
        CreateAbility(ref MagicBeam, magicBeamPrefab, MagicBeamId);
        CreateAbility(ref MagicBlast, magicBlastPrefab, MagicBlastId);

        // splash
        CreateAbility(ref SplashLob, splashLobPrefab, SplashLobId);
        CreateAbility(ref SplashVolley, splashVolleyPrefab, SplashVolleyId);
        CreateAbility(ref SplashBomb, splashBombPrefab, SplashBombId);

        // shield
        CreateAbility(ref ShieldBash, shieldBashPrefab, ShieldBashId);
        CreateAbility(ref shieldBlock, shieldBlockPrefab, ShieldBlockId);
        CreateAbility(ref ShieldWall, shieldWallPrefab, ShieldWallId);

        // unarmed
        CreateAbility(ref UnarmedPunch, unarmedPunchPrefab, UnarmedPunchId);
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("Player disconnected");

        if (Bootstrap.IsUnityEditor())
        {
            // check on application quit action reason
            if (null == NetworkManager.RpcTarget)
                return;
        }

        DestroyAbility(ref Dash);

        DestroyAbility(ref CleaveSlash);
        DestroyAbility(ref CleaveWhirlwind);
        DestroyAbility(ref CleaveCyclone);

        DestroyAbility(ref PierceThrust);
        DestroyAbility(ref PierceDrill);
        DestroyAbility(ref PierceLance);

        DestroyAbility(ref SmashSwipe);
        DestroyAbility(ref SmashWave);
        DestroyAbility(ref SmashSlam);

        DestroyAbility(ref BallisticShot);
        DestroyAbility(ref BallisticSnipe);
        DestroyAbility(ref BallisticExplosion);

        DestroyAbility(ref MagicCast);
        DestroyAbility(ref MagicBeam);
        DestroyAbility(ref MagicBlast);

        DestroyAbility(ref SplashLob);
        DestroyAbility(ref SplashVolley);
        DestroyAbility(ref SplashBomb);

        DestroyAbility(ref ShieldBash);
        DestroyAbility(ref shieldBlock);
        DestroyAbility(ref ShieldWall);

        DestroyAbility(ref UnarmedPunch);
    }

    void DestroyAbility(ref GameObject ability)
    {
        if (ability != null)
        {
            if (IsServer) ability.GetComponent<NetworkObject>().Despawn();
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (IsServer && IsSpawned)
        {
            leftAttackCooldown.Value -= dt;
            rightAttackCooldown.Value -= dt;
        }

        if (IsClient && IsSpawned)
        {
            // dash
            TryAddAbilityClientSide(ref Dash, DashId);

            // cleave
            TryAddAbilityClientSide(ref CleaveSlash, CleaveSlashId);
            TryAddAbilityClientSide(ref CleaveWhirlwind, CleaveWhirlwindId);
            TryAddAbilityClientSide(ref CleaveCyclone, CleaveCycloneId);

            // pierce
            TryAddAbilityClientSide(ref PierceThrust, PierceThrustId);
            TryAddAbilityClientSide(ref PierceDrill, PierceDrillId);
            TryAddAbilityClientSide(ref PierceLance, PierceLanceId);

            // smash
            TryAddAbilityClientSide(ref SmashSwipe, SmashSwipeId);
            TryAddAbilityClientSide(ref SmashWave, SmashWaveId);
            TryAddAbilityClientSide(ref SmashSlam, SmashSlamId);

            // ballistic
            TryAddAbilityClientSide(ref BallisticShot, BallisticShotId);
            TryAddAbilityClientSide(ref BallisticSnipe, BallisticSnipeId);
            TryAddAbilityClientSide(ref BallisticExplosion, BallisticExplosionId);

            // magic
            TryAddAbilityClientSide(ref MagicCast, MagicCastId);
            TryAddAbilityClientSide(ref MagicBeam, MagicBeamId);
            TryAddAbilityClientSide(ref MagicBlast, MagicBlastId);

            // splash
            TryAddAbilityClientSide(ref SplashLob, SplashLobId);
            TryAddAbilityClientSide(ref SplashVolley, SplashVolleyId);
            TryAddAbilityClientSide(ref SplashBomb, SplashBombId);

            // shield
            TryAddAbilityClientSide(ref ShieldBash, ShieldBashId);
            TryAddAbilityClientSide(ref shieldBlock, ShieldBlockId);
            TryAddAbilityClientSide(ref ShieldWall, ShieldWallId);

            // unarmed
            TryAddAbilityClientSide(ref UnarmedPunch, UnarmedPunchId);
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

    void CreateAbility(ref GameObject ability, GameObject prefab, NetworkVariable<ulong> abilityId)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab passed to CreateAbility in PlayerAbilities was null");
            return;
        }
        if (!prefab.HasComponent<PlayerAbility>())
        {
            Debug.LogWarning("Prefab for " + ability.ToString() + " ability is not a valid PlayerAbility");
            return;
        }
        ability = Instantiate(prefab);
        ability.GetComponent<NetworkObject>().Spawn();
        ability.GetComponent<NetworkObject>().TrySetParent(gameObject, false);
        ability.transform.localPosition = Vector3.zero;
        ability.transform.rotation = Quaternion.identity;
        abilityId.Value = ability.GetComponent<NetworkObject>().NetworkObjectId;
    }

    void TryAddAbilityClientSide(ref GameObject ability, NetworkVariable<ulong> abilityId)
    {
        if (ability == null && abilityId.Value > 0)
        {
            ability = NetworkManager.SpawnManager.SpawnedObjects[abilityId.Value].gameObject;
            if (ability == null)
            {
                Debug.LogWarning("Could not add ability " + ability.ToString() + " to the client side");
                return;
            }
            if (!ability.HasComponent<PlayerAbility>())
            {
                Debug.LogWarning("No PlayerAbility on " + ability.ToString() + " ability on client side");
                return;
            }
            ability.GetComponent<PlayerAbility>().Player = gameObject;
        }
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