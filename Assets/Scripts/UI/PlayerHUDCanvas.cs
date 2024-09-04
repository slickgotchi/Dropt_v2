using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDCanvas : MonoBehaviour
{
    private static PlayerHUDCanvas _singleton;

    public static PlayerHUDCanvas Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<PlayerHUDCanvas>();
                if (_singleton == null)
                {
                    Debug.LogError("There needs to be one active PlayerHUDCanvas script on a GameObject in your scene.");
                }
            }
            return _singleton;
        }
    }

    void Awake()
    {
        if (_singleton == null)
        {
            _singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_singleton != this)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private GameObject m_container;
    [SerializeField] private Slider m_healthSlider;
    [SerializeField] private TextMeshProUGUI m_healthText;
    [SerializeField] private Slider m_abilitySlider;
    [SerializeField] private TextMeshProUGUI m_abilityText;

    [SerializeField] private TextMeshProUGUI m_lhCooldownText;
    [SerializeField] private TextMeshProUGUI m_rhCooldownText;

    [SerializeField] private TextMeshProUGUI m_gltrText;

    [SerializeField] private TextMeshProUGUI m_essenceText;

    [SerializeField] private Image LHWearableImage;
    [SerializeField] private Image RHWearableImage;

    [SerializeField] private GameObject m_dungeonCollectibles;

    [SerializeField] private TMPro.TextMeshProUGUI m_levelNumber;
    [SerializeField] private TMPro.TextMeshProUGUI m_levelName;

    private NetworkCharacter m_localPlayerCharacter;

    public void SetLocalPlayerCharacter(NetworkCharacter localPlayerCharacter)
    {
        m_localPlayerCharacter = localPlayerCharacter;
    }

    public void SetLevelNumberAndName(string number, string name)
    {
        m_levelNumber.text = number;
        m_levelName.text = name;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_localPlayerCharacter == null)
        {
            m_container.SetActive(false);
            return;
        }

        m_container.SetActive(true);

        UpdateStatBars();
        UpdateCooldowns();
        UpdateGltr();
        UpdateEssence();
        UpdateAbilityIcons();

        if (Screen.fullScreen)
        {
            m_dungeonCollectibles.GetComponent<RectTransform>().anchoredPosition = new Vector3(-10, 10, 0);
        } else
        {
            m_dungeonCollectibles.GetComponent<RectTransform>().anchoredPosition = new Vector3(-10, 50, 0);
        }
    }

    void UpdateStatBars()
    {
        // HP
        var maxHp = m_localPlayerCharacter.HpMax.Value + m_localPlayerCharacter.HpBuffer.Value;
        var currHp = m_localPlayerCharacter.HpCurrent.Value;

        m_healthSlider.maxValue = maxHp;
        m_healthSlider.value = currHp;

        m_healthText.text = currHp.ToString("F0") + " / " + maxHp.ToString("F0");

        // AP
        var maxAp = m_localPlayerCharacter.ApMax.Value + m_localPlayerCharacter.ApBuffer.Value;
        var currAp = m_localPlayerCharacter.ApCurrent.Value;

        m_abilitySlider.maxValue = maxAp;
        m_abilitySlider.value = currAp;

        m_abilityText.text = currAp.ToString("F0") + " / " + maxAp.ToString("F0");
    }

    void UpdateCooldowns()
    {
        var lhRem = m_localPlayerCharacter.GetComponent<PlayerPrediction>().GetSpecialCooldownRemaining(Hand.Left);
        var rhRem = m_localPlayerCharacter.GetComponent<PlayerPrediction>().GetSpecialCooldownRemaining(Hand.Right);

        m_lhCooldownText.text = lhRem < 0.1f ? "" : lhRem.ToString("F0");
        m_rhCooldownText.text = rhRem < 0.1f ? "" : rhRem.ToString("F0");
    }

    void UpdateGltr()
    {
        var gltrCount = m_localPlayerCharacter.GetComponent<PlayerDungeonData>().SpiritDust;
        m_gltrText.text = gltrCount.Value.ToString();
    }


    void UpdateEssence()
    {
        var essence = m_localPlayerCharacter.GetComponent<PlayerDungeonData>().Essence;
        m_essenceText.text = essence.Value.ToString("F0");
    }

    Wearable.NameEnum lhOld;
    Wearable.NameEnum rhOld;

    void UpdateAbilityIcons()
    {
        var equipment = m_localPlayerCharacter.GetComponent<PlayerEquipment>();
        if (equipment == null) return;

        var lhEnum = equipment.LeftHand.Value;
        var rhEnum = equipment.RightHand.Value;

        if (lhEnum != lhOld)
        {
            LHWearableImage.sprite = WeaponSpriteManager.Instance.GetSprite(lhEnum, PlayerGotchi.Facing.Front);
            lhOld = lhEnum;
        }

        if (rhEnum != rhOld)
        {
            RHWearableImage.sprite = WeaponSpriteManager.Instance.GetSprite(rhEnum, PlayerGotchi.Facing.Front);
            rhOld = rhEnum;
        }
    }
}
