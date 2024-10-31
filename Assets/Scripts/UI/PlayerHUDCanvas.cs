using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

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
    [SerializeField] private Image m_hpImage;
    [SerializeField] private TextMeshProUGUI m_hpText;
    [SerializeField] private Image m_apImage;
    [SerializeField] private TextMeshProUGUI m_apText;

    [SerializeField] private TextMeshProUGUI m_lhCooldownText;
    [SerializeField] private TextMeshProUGUI m_rhCooldownText;

    [SerializeField] private TextMeshProUGUI m_bombsText;
    [SerializeField] private TextMeshProUGUI m_dustText;
    [SerializeField] private TextMeshProUGUI m_ectoText;

    [SerializeField] private TextMeshProUGUI m_essenceText;
    [SerializeField] private Image m_essenceImage;

    [SerializeField] private Image LHWearableImage;
    [SerializeField] private Image RHWearableImage;

    [SerializeField] private GameObject m_dungeonCollectibles;

    [SerializeField] private TextMeshProUGUI m_levelNumber;
    [SerializeField] private TextMeshProUGUI m_levelName;

    private PlayerCharacter m_localPlayerCharacter;
    private PlayerOffchainData m_localPlayerDungeonData;

    [SerializeField] private Slider m_leftHandShieldBar;
    [SerializeField] private Slider m_rightHandShieldBar;

    [SerializeField] private PetMeterView m_PetMeterView;


    public void SetLocalPlayerCharacter(PlayerCharacter localPlayerCharacter)
    {
        m_localPlayerCharacter = localPlayerCharacter;
        m_localPlayerDungeonData = localPlayerCharacter.GetComponent<PlayerOffchainData>();
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

        UpdateBombs();
        UpdateDust();
        UpdateEcto();

        UpdateEssence();
        UpdateAbilityIcons();
    }

    void UpdateStatBars()
    {
        // HP
        var maxHp = m_localPlayerCharacter.HpMax.Value + m_localPlayerCharacter.HpBuffer.Value;
        var currHp = m_localPlayerCharacter.HpCurrent.Value;

        m_hpImage.fillAmount = currHp / maxHp;

        m_hpText.text = currHp.ToString("F0") + " / " + maxHp.ToString("F0");

        // AP
        var maxAp = m_localPlayerCharacter.ApMax.Value + m_localPlayerCharacter.ApBuffer.Value;
        var currAp = m_localPlayerCharacter.ApCurrent.Value;

        m_apImage.fillAmount = currAp / maxAp;

        m_apText.text = currAp.ToString("F0") + " / " + maxAp.ToString("F0");
    }

    void UpdateCooldowns()
    {
        var lhRem = m_localPlayerCharacter.GetComponent<PlayerPrediction>().GetSpecialCooldownRemaining(Hand.Left);
        var rhRem = m_localPlayerCharacter.GetComponent<PlayerPrediction>().GetSpecialCooldownRemaining(Hand.Right);

        // round up
        lhRem = math.ceil(lhRem);
        rhRem = math.ceil(rhRem);

        m_lhCooldownText.text = lhRem < 0.1f ? "" : lhRem.ToString("F0");
        m_rhCooldownText.text = rhRem < 0.1f ? "" : rhRem.ToString("F0");
    }

    void UpdateDust()
    {
        var dust = LevelManager.Instance.IsDegenapeVillage() ? m_localPlayerDungeonData.dustBalance_offchain : m_localPlayerDungeonData.dustCount_dungeon;
        m_dustText.text = dust.Value.ToString();
    }

    void UpdateBombs()
    {
        var bombs = LevelManager.Instance.IsDegenapeVillage() ? m_localPlayerDungeonData.bombBalance_offchain : m_localPlayerDungeonData.bombCount_dungeon;
        m_bombsText.text = bombs.Value.ToString("F0");
    }

    void UpdateEcto()
    {
        var ecto = LevelManager.Instance.IsDegenapeVillage() ? m_localPlayerDungeonData.ectoBalance_offchain : m_localPlayerDungeonData.ectoCount_dungeon;
        m_ectoText.text = ecto.Value.ToString("F0");
    }

    void UpdateEssence()
    {
        var essence = m_localPlayerCharacter.Essence;
        m_essenceText.text = essence.Value.ToString("F0");
        m_essenceImage.fillAmount = essence.Value / 1000;
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

    public void VisibleShieldBar(Hand hand, bool isVisible)
    {
        if (hand == Hand.Left)
        {
            m_leftHandShieldBar.gameObject.SetActive(isVisible);
        }
        else
        {
            m_rightHandShieldBar.gameObject.SetActive(isVisible);
        }
    }

    public void SetShieldBarProgress(Hand hand, float progress)
    {
        if (hand == Hand.Left)
        {
            m_leftHandShieldBar.value = progress;
        }
        else
        {
            m_rightHandShieldBar.value = progress;
        }
    }

    public void ActivatePetMeter(Sprite pet)
    {
        m_PetMeterView.Activate(pet);
    }

    public void DeactivatePetMeter()
    {
        m_PetMeterView.Deactivate();
    }

    public void SetPetMeterProgress(float progress)
    {
        m_PetMeterView.SetProgress(progress);
    }
}
