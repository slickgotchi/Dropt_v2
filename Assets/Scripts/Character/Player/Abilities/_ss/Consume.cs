using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class Consume : PlayerAbility
{
    [Header("Buff Timer Prefab")]
    public GameObject BuffTimerPrefab;

    [Header("Consume Buff Objects")]
    public BuffObject AppleJuice_SpecialBuff;
    public BuffObject BedtimeMilk_SpecialBuff;
    public BuffObject GMSeeds_SpecialBuff;
    public BuffObject GotchiMug_SpecialBuff;
    public BuffObject LilPumpDrank_SpecialBuff;
    public BuffObject LinkBubbly_SpecialBuff;
    public BuffObject Martini_SpecialBuff;
    public BuffObject Milkshake_SpecialBuff;
    public BuffObject UraniumRod_SpecialBuff;
    public BuffObject WaterBottle_SpecialBuff;
    public BuffObject WaterJug_SpecialBuff;
    public BuffObject Wine_SpecialBuff;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnStart()
    {
        // get player to do consume animation
        var hand = ActivationInput.abilityHand;
        if (hand == Hand.Left)
        {
            Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchiLeftHand_Consume");
        } else
        {
            Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchiRightHand_Consume");
        }

        if (!IsServer) return;

        if (hand == Hand.Left)
        {
            var wearableNameEnum = Player.GetComponent<PlayerEquipment>().LeftHand.Value;
            ApplyBuff(wearableNameEnum);
        }
        else
        {
            var wearableNameEnum = Player.GetComponent<PlayerEquipment>().RightHand.Value;
            ApplyBuff(wearableNameEnum);
        }
    }

    void ApplyBuff(Wearable.NameEnum wearableNameEnum)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);
        BuffObject buffObject = null;

        switch (wearableNameEnum)
        {
            case Wearable.NameEnum.AppleJuice: buffObject = AppleJuice_SpecialBuff; break;
            case Wearable.NameEnum.BedtimeMilk: buffObject = BedtimeMilk_SpecialBuff; break;
            case Wearable.NameEnum.GMSeeds: buffObject = GMSeeds_SpecialBuff; break;
            case Wearable.NameEnum.GotchiMug: buffObject = GotchiMug_SpecialBuff; break;
            case Wearable.NameEnum.LilPumpDrank: buffObject = LilPumpDrank_SpecialBuff; break;
            case Wearable.NameEnum.LinkBubbly: buffObject = LinkBubbly_SpecialBuff; break;
            case Wearable.NameEnum.Martini: buffObject = Martini_SpecialBuff; break;
            case Wearable.NameEnum.Milkshake: buffObject = Milkshake_SpecialBuff; break;
            case Wearable.NameEnum.UraniumRod: buffObject = UraniumRod_SpecialBuff; break;
            case Wearable.NameEnum.Waterbottle: buffObject = WaterBottle_SpecialBuff; break;
            case Wearable.NameEnum.WaterJug: buffObject = WaterJug_SpecialBuff; break;
            case Wearable.NameEnum.Wine: buffObject = Wine_SpecialBuff; break;
            default: break;
        }

        if (buffObject == null) return;

        var buffTimer = Instantiate(BuffTimerPrefab).GetComponent<BuffTimer>();
        buffTimer.StartBuff(buffObject, Player.GetComponent<NetworkCharacter>(), wearable.EffectDuration);
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
        Player.GetComponent<PlayerGotchi>().ResetIdleAnimation();
    }
}
