using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class Aura : PlayerAbility
{
    [Header("Aura Parameters")]
    public GameObject BuffAlliesTimerPrefab;

    [Header("Consume Buff Objects")]
    public BuffObject AaveFlag_SpecialBuff;
    public BuffObject JamaicanFlag_SpecialBuff;
    public BuffObject L2Sign_SpecialBuff;
    public BuffObject OKExSign_SpecialBuff;
    public BuffObject RektSign_SpecialBuff;
    public BuffObject UpArrow_SpecialBuff;
    public BuffObject VoteSign_SpecialBuff;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnStart()
    {
        // set local rotation/position.
        // IMPORTANT SetRotation(), SetRotationToActionDirection() and SetLocalPosition() must be used as
        // they call RPC's that sync remote clients
        SetRotation(quaternion.identity);
        SetLocalPosition(PlayerAbilityCentreOffset);

        // IMPORTANT use PlayAnimation which calls RPC's in the background that play the 
        // animation on remote clients
        PlayAnimation("Aura");

        // get player to do consume animation
        var hand = ActivationInput.abilityHand;
        if (hand == Hand.Left)
        {
            Player.GetComponent<PlayerGotchi>().PlayAnimation("PlayerGotchiLeftHand_Consume");
        }
        else
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
            case Wearable.NameEnum.AaveFlag: 
                buffObject = AuraManager.Instance.IsAaveFlagActive ? null : AaveFlag_SpecialBuff; 
                break;
            case Wearable.NameEnum.JamaicanFlag: 
                buffObject = AuraManager.Instance.IsJamaicanFlagActive ? null : JamaicanFlag_SpecialBuff; 
                break;
            case Wearable.NameEnum.L2Sign: 
                buffObject = AuraManager.Instance.IsL2SignActive ? null : L2Sign_SpecialBuff; 
                break;
            case Wearable.NameEnum.OKexSign: 
                buffObject = AuraManager.Instance.IsOKExSignActive ? null : OKExSign_SpecialBuff; 
                break;
            case Wearable.NameEnum.REKTSign: 
                buffObject = AuraManager.Instance.IsRektSignActive ? null : RektSign_SpecialBuff;
                break;
            case Wearable.NameEnum.UpArrow: 
                buffObject = AuraManager.Instance.IsUpArrowActive ? null : UpArrow_SpecialBuff; 
                break;
            case Wearable.NameEnum.VoteSign: 
                buffObject = AuraManager.Instance.IsVoteSignActive ? null : VoteSign_SpecialBuff; 
                break;
            default: break;
        }
        if (buffObject == null) return;

        var buffTimer = Instantiate(BuffAlliesTimerPrefab).GetComponent<BuffAlliesTimer>();
        buffTimer.StartBuff(buffObject, wearable.EffectDuration);
    }

    public override void OnUpdate()
    {

    }

    public override void OnFinish()
    {
        Player.GetComponent<PlayerGotchi>().ResetIdleAnimation();
    }
}
