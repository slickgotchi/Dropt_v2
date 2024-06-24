/*
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerAbilities : NetworkBehaviour
{
    [SerializeField]
    private List<GameObject> abilityPrefabs = new List<GameObject>();
    private Dictionary<PlayerAbilityEnum, GameObject> instantiatedAbilities = new Dictionary<PlayerAbilityEnum, GameObject>();
    private Dictionary<PlayerAbilityEnum, NetworkVariable<ulong>> abilityIds = new Dictionary<PlayerAbilityEnum, NetworkVariable<ulong>>();

    public NetworkVariable<float> leftAttackCooldown = new NetworkVariable<float>(0);
    public NetworkVariable<float> rightAttackCooldown = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Ensure we instantiate here in Start() AFTER OnNetworkSpawn() otherwise we end up with duplicates
        foreach (var prefab in abilityPrefabs)
        {
            PlayerAbility ability = prefab.GetComponent<PlayerAbility>();
            if (ability != null)
            {
                PlayerAbilityEnum abilityEnum = (PlayerAbilityEnum)System.Enum.Parse(typeof(PlayerAbilityEnum), prefab.name);
                var instantiatedAbility = Instantiate(prefab);
                instantiatedAbility.GetComponent<NetworkObject>().Spawn();
                instantiatedAbilities[abilityEnum] = instantiatedAbility;
                abilityIds[abilityEnum] = new NetworkVariable<ulong>(instantiatedAbility.GetComponent<NetworkObject>().NetworkObjectId);
            }
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
            foreach (var abilityEnum in abilityIds.Keys.ToList())
            {
                if (instantiatedAbilities[abilityEnum] == null && abilityIds[abilityEnum].Value > 0)
                {
                    var spawnedObject = NetworkManager.SpawnManager.SpawnedObjects[abilityIds[abilityEnum].Value].gameObject;
                    instantiatedAbilities[abilityEnum] = spawnedObject;
                    spawnedObject.GetComponent<PlayerAbility>().Player = gameObject;
                }
            }
        }
    }

    public PlayerAbility GetAbility(PlayerAbilityEnum abilityEnum)
    {
        if (!IsSpawned || !instantiatedAbilities.ContainsKey(abilityEnum)) return null;
        return instantiatedAbilities[abilityEnum].GetComponent<PlayerAbility>();
    }

    public PlayerAbilityEnum GetAttackAbilityEnum(Wearable.NameEnum wearableNameEnum)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Cleave) return PlayerAbilityEnum.CleaveSwing;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Smash) return PlayerAbilityEnum.SmashSwing;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Pierce) return PlayerAbilityEnum.PierceThrust;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Ballistic) return PlayerAbilityEnum.BallisticShot;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Magic) return PlayerAbilityEnum.MagicCast;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Splash) return PlayerAbilityEnum.SplashLob;
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

        return PlayerAbilityEnum.Null;
    }

#if UNITY_EDITOR
    [ContextMenu("Populate Ability Prefabs")]
    private void PopulateAbilityPrefabs()
    {
        abilityPrefabs.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponent<PlayerAbility>() != null)
            {
                abilityPrefabs.Add(prefab);
            }
        }
    }
#endif
}

public enum Hand { Left, Right };

*/







using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilities : NetworkBehaviour
{
    public GameObject dashPrefab;
    [HideInInspector] public GameObject Dash;
    private NetworkVariable<ulong> DashId = new NetworkVariable<ulong>(0);

    public GameObject cleaveSwingPrefab;
    [HideInInspector] public GameObject CleaveSwing;
    private NetworkVariable<ulong> CleaveSwingId = new NetworkVariable<ulong>(0);

    public GameObject cleaveWhirlwindPrefab;
    [HideInInspector] public GameObject CleaveWhirlwind;
    private NetworkVariable<ulong> CleaveWhirlwindId = new NetworkVariable<ulong>(0);

    public GameObject cleaveCyclonePrefab;
    [HideInInspector] public GameObject CleaveCyclone;
    private NetworkVariable<ulong> CleaveCycloneId = new NetworkVariable<ulong>(0);

    public GameObject pierceThrustPrefab;
    [HideInInspector] public GameObject PierceThrust;
    private NetworkVariable<ulong> PierceThrustId = new NetworkVariable<ulong>(0);

    public GameObject pierceDrillPrefab;
    [HideInInspector] public GameObject PierceDrill;
    private NetworkVariable<ulong> PierceDrillId = new NetworkVariable<ulong>(0);

    public GameObject pierceLancePrefab;
    [HideInInspector] public GameObject PierceLance;
    private NetworkVariable<ulong> PierceLanceId = new NetworkVariable<ulong>(0);

    public NetworkVariable<float> leftAttackCooldown = new NetworkVariable<float>(0);
    public NetworkVariable<float> rightAttackCooldown = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

    }

    private void Start()
    {
        // Ensure we instatiate here in Start() AFTER OnNetworkSpawn() otherwise we end up with duplciates
        if (!IsServer) return;

        Dash = Instantiate(dashPrefab);
        Dash.GetComponent<NetworkObject>().Spawn();
        DashId.Value = Dash.GetComponent<NetworkObject>().NetworkObjectId;

        CleaveSwing = Instantiate(cleaveSwingPrefab);
        CleaveSwing.GetComponent<NetworkObject>().Spawn();
        CleaveSwingId.Value = CleaveSwing.GetComponent<NetworkObject>().NetworkObjectId;

        CleaveWhirlwind = Instantiate(cleaveWhirlwindPrefab);
        CleaveWhirlwind.GetComponent<NetworkObject>().Spawn();
        CleaveWhirlwindId.Value = CleaveWhirlwind.GetComponent<NetworkObject>().NetworkObjectId;

        CleaveCyclone = Instantiate(cleaveCyclonePrefab);
        CleaveCyclone.GetComponent<NetworkObject>().Spawn();
        CleaveCycloneId.Value = CleaveCyclone.GetComponent<NetworkObject>().NetworkObjectId;

        PierceThrust = Instantiate(pierceThrustPrefab);
        PierceThrust.GetComponent<NetworkObject>().Spawn();
        PierceThrustId.Value = PierceThrust.GetComponent<NetworkObject>().NetworkObjectId;

        PierceDrill = Instantiate(pierceDrillPrefab);
        PierceDrill.GetComponent<NetworkObject>().Spawn();
        PierceDrillId.Value = PierceDrill.GetComponent<NetworkObject>().NetworkObjectId;

        PierceLance = Instantiate(pierceLancePrefab);
        PierceLance.GetComponent<NetworkObject>().Spawn();
        PierceLanceId.Value = PierceLance.GetComponent<NetworkObject>().NetworkObjectId;
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
            if (Dash == null && DashId.Value > 0)
            {
                Dash = NetworkManager.SpawnManager.SpawnedObjects[DashId.Value].gameObject;
                Dash.GetComponent<PlayerAbility>().Player = gameObject;
            }
            if (CleaveSwing == null && CleaveSwingId.Value > 0)
            {
                CleaveSwing = NetworkManager.SpawnManager.SpawnedObjects[CleaveSwingId.Value].gameObject;
                CleaveSwing.GetComponent<PlayerAbility>().Player = gameObject;
            }
            if (CleaveWhirlwind == null && CleaveWhirlwindId.Value > 0)
            {
                CleaveWhirlwind = NetworkManager.SpawnManager.SpawnedObjects[CleaveWhirlwindId.Value].gameObject;
                CleaveWhirlwind.GetComponent<PlayerAbility>().Player = gameObject;
            }
            if (CleaveCyclone == null && CleaveCycloneId.Value > 0)
            {
                CleaveCyclone = NetworkManager.SpawnManager.SpawnedObjects[CleaveCycloneId.Value].gameObject;
                CleaveCyclone.GetComponent<PlayerAbility>().Player = gameObject;
            }
            if (PierceThrust == null && PierceThrustId.Value > 0)
            {
                PierceThrust = NetworkManager.SpawnManager.SpawnedObjects[PierceThrustId.Value].gameObject;
                PierceThrust.GetComponent<PlayerAbility>().Player = gameObject;
            }
            if (PierceDrill == null && PierceDrillId.Value > 0)
            {
                PierceDrill = NetworkManager.SpawnManager.SpawnedObjects[PierceDrillId.Value].gameObject;
                PierceDrill.GetComponent<PlayerAbility>().Player = gameObject;
            }
            if (PierceLance == null && PierceLanceId.Value > 0)
            {
                PierceLance = NetworkManager.SpawnManager.SpawnedObjects[PierceLanceId.Value].gameObject;
                PierceLance.GetComponent<PlayerAbility>().Player = gameObject;
            }
        }
    }

    public PlayerAbility GetAbility(PlayerAbilityEnum abilityEnum)
    {
        if (!IsSpawned) return null;

        if (abilityEnum == PlayerAbilityEnum.Dash) return Dash.GetComponent<PlayerAbility>();

        if (abilityEnum == PlayerAbilityEnum.CleaveSwing) return CleaveSwing.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.CleaveWhirlwind) return CleaveWhirlwind.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.CleaveCyclone) return CleaveCyclone.GetComponent<PlayerAbility>();

        if (abilityEnum == PlayerAbilityEnum.PierceThrust) return PierceThrust.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.PierceDrill) return PierceDrill.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.PierceLance) return PierceLance.GetComponent<PlayerAbility>();

        return null;
    }

    public PlayerAbilityEnum GetAttackAbilityEnum(Wearable.NameEnum wearableNameEnum)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Cleave) return PlayerAbilityEnum.CleaveSwing;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Smash) return PlayerAbilityEnum.SmashSwing;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Pierce) return PlayerAbilityEnum.PierceThrust;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Ballistic) return PlayerAbilityEnum.BallisticShot;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Magic) return PlayerAbilityEnum.MagicCast;
        if (wearable.WeaponType == Wearable.WeaponTypeEnum.Splash) return PlayerAbilityEnum.SplashLob;
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

        return PlayerAbilityEnum.Null;
    }
}

public enum Hand { Left, Right };

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