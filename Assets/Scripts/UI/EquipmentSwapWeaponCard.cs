using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSwapWeaponCard : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Outline m_outline;

    [Header("Left Panel")]
    [SerializeField] private Image m_image;
    [SerializeField] private TextMeshProUGUI m_dmgText;
    [SerializeField] private TextMeshProUGUI m_typeText;
    [SerializeField] private TextMeshProUGUI m_rarityText;

    [Header("Right Panel - Name")]
    [SerializeField] private TextMeshProUGUI m_nameText;

    [Header("Right Panel - Attacks")]
    [SerializeField] private TextMeshProUGUI m_attackText;
    [SerializeField] private TextMeshProUGUI m_chargeText;
    [SerializeField] private TextMeshProUGUI m_specialText;

    [Header("Right Panel - Stats")]
    [SerializeField] private TextMeshProUGUI m_hpText;
    [SerializeField] private TextMeshProUGUI m_atkText;
    [SerializeField] private TextMeshProUGUI m_critText;
    [SerializeField] private TextMeshProUGUI m_apText;


    public void Init(Wearable.NameEnum wearableNameEnum, int gotchiId)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableNameEnum);
        if (wearable == null)
        {
            Debug.LogError("Invalid wearableNameEnum passed to EquipmentSwapWeaponCard Init()");
            return;
        }

        if (wearable.WeaponType == Wearable.WeaponTypeEnum.NA)
        {
            Debug.LogError("wearableNameEnum passed to EquipmentSwapWeaponCard Init() is not a weapon");
            return;
        }

        // general
        m_outline.effectColor = wearable.RarityColor;

        // left panel
        m_image.sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, wearable.AttackView);

        // calc stat buffs and damage
        var hp = DroptStatCalculator.GetDroptStatByWearableId(wearable.Id, TraitType.NRG);
        var atk = DroptStatCalculator.GetDroptStatByWearableId(wearable.Id, TraitType.AGG);
        var crit = DroptStatCalculator.GetDroptStatByWearableId(wearable.Id, TraitType.SPK);
        var ap = DroptStatCalculator.GetDroptStatByWearableId(wearable.Id, TraitType.BRN);

        var damage = wearable != null ?
            DroptStatCalculator.GetDroptStatForGotchiAndAllWearablesByGotchiId(gotchiId, TraitType.AGG) * wearable.RarityMultiplier * GetBaseAttackMultiplier(wearable.WeaponType)
            : 0;

        m_dmgText.text = damage.ToString("F0") + " dmg";
        m_typeText.text = wearable.WeaponType.ToString();
        m_rarityText.text = wearable.Rarity.ToString();

        // right panel
        m_nameText.text = wearable.Name;
        m_attackText.text = wearable.AttackDescription;
        m_chargeText.text = wearable.HoldDescription;
        m_specialText.text = wearable.SpecialDescription;

        m_hpText.text = "+" + hp.ToString("F0") + " Health";
        m_atkText.text = "+" + atk.ToString("F0") + " Attack";
        m_critText.text = "+" + (crit * 100).ToString("F1") + " Critical %";
        m_apText.text = "+" + ap.ToString("F0") + " Ability Pts";

        // set some text colorisations
        var activeTextColor = Dropt.Utils.Color.HexToColor("#ffffff");
        var inactiveTextColor = Dropt.Utils.Color.HexToColor("#858585");

        m_hpText.color = hp <= 0.1f ? inactiveTextColor : activeTextColor;
        m_atkText.color = atk <= 0.1f ? inactiveTextColor : activeTextColor;
        m_critText.color = crit < 0.01f ? inactiveTextColor : activeTextColor;
        m_apText.color = ap < 0.1f ? inactiveTextColor : activeTextColor;
    }

    public float GetBaseAttackMultiplier(Wearable.WeaponTypeEnum weaponType)
    {
        switch (weaponType)
        {
            case Wearable.WeaponTypeEnum.Unarmed:
                var unarmedPunch = FindAnyObjectByType<UnarmedPunch>();
                return unarmedPunch != null ? unarmedPunch.DamageMultiplier : 1;
            case Wearable.WeaponTypeEnum.Cleave:
                var cleaveSlash = FindAnyObjectByType<CleaveSlash>();
                return cleaveSlash != null ? cleaveSlash.DamageMultiplier : 1;
            case Wearable.WeaponTypeEnum.Smash:
                var smashSwipe = FindAnyObjectByType<SmashSwipe>();
                return smashSwipe != null ? smashSwipe.DamageMultiplier : 1;
            case Wearable.WeaponTypeEnum.Pierce:
                var pierceThrust = FindAnyObjectByType<PierceThrust>();
                return pierceThrust != null ? pierceThrust.DamageMultiplier : 1;
            case Wearable.WeaponTypeEnum.Ballistic:
                var ballisticShot = FindAnyObjectByType<BallisticShot>();
                return ballisticShot != null ? ballisticShot.DamageMultiplier : 1;
            case Wearable.WeaponTypeEnum.Magic:
                var magicCast = FindAnyObjectByType<MagicCast>();
                return magicCast != null ? magicCast.DamageMultiplier : 1;
            case Wearable.WeaponTypeEnum.Splash:
                var splashLob = FindAnyObjectByType<SplashLob>();
                return splashLob != null ? splashLob.DamageMultiplier : 1;
            case Wearable.WeaponTypeEnum.Shield:
                var shieldBash = FindAnyObjectByType<ShieldBash>();
                return shieldBash != null ? shieldBash.DamageMultiplier : 1;
            default: break;
        }

        return 1;
    }
}
