using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwapCanvas : MonoBehaviour
{
    public static WeaponSwapCanvas Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Container.SetActive(false);
    }

    public GameObject Container;

    public Image WeaponImage;
    public TextMeshProUGUI WeaponNameText;

    public TextMeshProUGUI TypeText;
    public TextMeshProUGUI DamageText;
    public TextMeshProUGUI SpecialApText;

    public TextMeshProUGUI BaseDescriptionText;
    public TextMeshProUGUI AttackDescriptionText;
    public TextMeshProUGUI HoldDescriptionText;
    public TextMeshProUGUI SpecialDescriptionText;

    //public TextMeshProUGUI SpecialCooldownText;

    public bool IsActive()
    {
        return Container.activeSelf;
    }

    public void Init(Wearable.NameEnum wearableName)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableName);
        var sprite = WeaponSpriteManager.Instance.GetSprite(wearableName, PlayerGotchi.Facing.Right);

        WeaponImage.sprite = sprite;
        WeaponNameText.text = wearable.Name;

        TypeText.text = "Type: " + wearable.WeaponType.ToString();
        DamageText.text = "Damage Multiplier: " + wearable.RarityMultiplier + "x";

        BaseDescriptionText.text = wearable.BaseDescription;
        AttackDescriptionText.text = wearable.AttackDescription;
        HoldDescriptionText.text = wearable.HoldDescription;
        SpecialDescriptionText.text = wearable.SpecialDescription;
        SpecialApText.text = "AP Cost: " + wearable.SpecialAp.ToString();

        //SpecialCooldownText.text = "Cooldown: " + wearable.SpecialCooldown.ToString() + "s";

        Container.transform.Find("Layout").GetComponent<Outline>().effectColor = wearable.RarityColor;
    }
}
