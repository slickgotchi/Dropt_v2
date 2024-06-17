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

        var playerId = GetComponent<NetworkObject>().NetworkObjectId;

        Dash = Instantiate(dashPrefab);
        Dash.GetComponent<NetworkObject>().Spawn();
        Dash.GetComponent<PlayerAbility>().PlayerNetworkObjectId.Value = playerId;
        //DashId.Value = Dash.GetComponent<NetworkObject>().NetworkObjectId;

        CleaveSwing = Instantiate(cleaveSwingPrefab);
        CleaveSwing.GetComponent<NetworkObject>().Spawn();
        CleaveSwing.GetComponent<PlayerAbility>().PlayerNetworkObjectId.Value = playerId;
        //CleaveSwingId.Value = Dash.GetComponent<NetworkObject>().NetworkObjectId;

        PierceThrust = Instantiate(pierceThrustPrefab);
        PierceThrust.GetComponent<NetworkObject>().Spawn();
        PierceThrust.GetComponent<PlayerAbility>().PlayerNetworkObjectId.Value = playerId;
        //PierceThrustId.Value = Dash.GetComponent<NetworkObject>().NetworkObjectId;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        //if (IsServer && IsSpawned)
        //{
        //    leftAttackCooldown.Value -= dt;
        //    rightAttackCooldown.Value -= dt;
        //}
    }

    public PlayerAbility GetAbility(PlayerAbilityEnum abilityEnum)
    {
        //if (abilityEnum == PlayerAbilityEnum.Dash) return Dash.GetComponent<PlayerAbility>();
        //if (abilityEnum == PlayerAbilityEnum.CleaveSwing) return CleaveSwing.GetComponent<PlayerAbility>();
        //if (abilityEnum == PlayerAbilityEnum.PierceThrust) return PierceThrust.GetComponent<PlayerAbility>();

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