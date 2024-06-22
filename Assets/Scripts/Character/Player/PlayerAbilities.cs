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

    public GameObject pierceThrustPrefab;
    [HideInInspector] public GameObject PierceThrust;
    private NetworkVariable<ulong> PierceThrustId = new NetworkVariable<ulong>(0);

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

        PierceThrust = Instantiate(pierceThrustPrefab);
        PierceThrust.GetComponent<NetworkObject>().Spawn();
        PierceThrustId.Value = PierceThrust.GetComponent<NetworkObject>().NetworkObjectId;
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
            if (PierceThrust == null && PierceThrustId.Value > 0)
            {
                PierceThrust = NetworkManager.SpawnManager.SpawnedObjects[PierceThrustId.Value].gameObject;
                PierceThrust.GetComponent<PlayerAbility>().Player = gameObject;
            }
        }
    }

    public PlayerAbility GetAbility(PlayerAbilityEnum abilityEnum)
    {
        if (!IsSpawned) return null;

        if (abilityEnum == PlayerAbilityEnum.Dash) return Dash.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.CleaveSwing) return CleaveSwing.GetComponent<PlayerAbility>();
        if (abilityEnum == PlayerAbilityEnum.PierceThrust) return PierceThrust.GetComponent<PlayerAbility>();

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

}

public enum Hand { Left, Right };