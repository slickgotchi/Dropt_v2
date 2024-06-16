using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    public Dash dash;
    public CleaveSwing cleaveSwing;
    public PierceThrust pierceThrust;

    private void Awake()
    {
        dash = Instantiate(dash);
        dash.GetComponent<NetworkObject>().Spawn();

        cleaveSwing = Instantiate(cleaveSwing);
        cleaveSwing.GetComponent<NetworkObject>().Spawn();

        pierceThrust = Instantiate(pierceThrust);
        pierceThrust.GetComponent<NetworkObject>().Spawn(); 
    }

    private void Update()
    {
        if (cleaveSwing != null)
        {
            //if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
            //{
            //    cleaveSwing.PerformCleaveSwing(transform.position);
            //}
        }
    }

    public PlayerAbility GetAbility(PlayerAbilityEnum abilityEnum)
    {
        if (abilityEnum == PlayerAbilityEnum.Dash) return dash;
        if (abilityEnum == PlayerAbilityEnum.CleaveSwing) return cleaveSwing;
        if (abilityEnum == PlayerAbilityEnum.PierceThrust) return pierceThrust;

        return null;
    }

    public PlayerAbilityEnum GetAttackAbility(Wearable.NameEnum wearableNameEnum)
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

    //public PlayerAbilityEnum GetRightAttackAbility(Wearable.NameEnum wearableNameEnum)
    //{
    //    var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);

    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Cleave) return PlayerAbilityEnum.CleaveSwing;
    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Smash) return PlayerAbilityEnum.SmashSwing;
    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Pierce) return PlayerAbilityEnum.PierceThrust;
    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Ballistic) return PlayerAbilityEnum.BallisticShot;
    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Magic) return PlayerAbilityEnum.MagicCast;
    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Splash) return PlayerAbilityEnum.SplashLob;
    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Shield) return PlayerAbilityEnum.ShieldBash;
    //    if (wearable.WeaponType == Wearable.WeaponTypeEnum.Unarmed) return PlayerAbilityEnum.Unarmed;

    //    return PlayerAbilityEnum.Null;
    //}
}
