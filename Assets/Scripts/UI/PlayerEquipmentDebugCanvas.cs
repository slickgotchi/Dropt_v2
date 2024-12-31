using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerEquipmentDebugCanvas : DroptCanvas
{
    public static PlayerEquipmentDebugCanvas Instance { get; private set; }

    private PlayerEquipment playerEquipment;

    public TMP_Dropdown bodyDropdown;
    public TMP_Dropdown faceDropdown;
    public TMP_Dropdown eyesDropdown;
    public TMP_Dropdown headDropdown;
    public TMP_Dropdown rightHandWeaponDropdown;
    public TMP_Dropdown rightHandWearableDropdown;
    public TMP_Dropdown leftHandWeaponDropdown;
    public TMP_Dropdown leftHandWearableDropdown;
    public TMP_Dropdown petDropdown;


    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InstaHideCanvas();
    }

    bool m_isInitialized = false;

    public override void OnUpdate()
    {
        FindLatestPlayerEquipment();
        if (playerEquipment != null)
        {
            if (!m_isInitialized)
            {
                InitializeDropdowns();
                SetDropdownValues();
                SetUpDropdownListeners();
                m_isInitialized = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (PlayerEquipmentDebugCanvas.Instance.isCanvasOpen)
            {
                HideCanvas();
            } else
            {
                ShowCanvas();
            }
        }
    }

    private void InitializeDropdowns()
    {
        InitializeDropdown(bodyDropdown, Wearable.SlotEnum.Body);
        InitializeDropdown(faceDropdown, Wearable.SlotEnum.Face);
        InitializeDropdown(eyesDropdown, Wearable.SlotEnum.Eyes);
        InitializeDropdown(headDropdown, Wearable.SlotEnum.Head);
        InitializeWeaponTypeDropdown(rightHandWeaponDropdown);
        InitializeDropdown(rightHandWearableDropdown, Wearable.SlotEnum.Hand);
        InitializeWeaponTypeDropdown(leftHandWeaponDropdown);
        InitializeDropdown(leftHandWearableDropdown, Wearable.SlotEnum.Hand);
        InitializeDropdown(petDropdown, Wearable.SlotEnum.Pet);
    }

    private void SetUpDropdownListeners()
    {
        bodyDropdown.onValueChanged.AddListener(index => OnDropdownChanged(bodyDropdown, Wearable.SlotEnum.Body));
        faceDropdown.onValueChanged.AddListener(index => OnDropdownChanged(faceDropdown, Wearable.SlotEnum.Face));
        eyesDropdown.onValueChanged.AddListener(index => OnDropdownChanged(eyesDropdown, Wearable.SlotEnum.Eyes));
        headDropdown.onValueChanged.AddListener(index => OnDropdownChanged(headDropdown, Wearable.SlotEnum.Head));
        rightHandWeaponDropdown.onValueChanged.AddListener(index => OnWeaponDropdownChanged(rightHandWeaponDropdown, rightHandWearableDropdown));
        rightHandWearableDropdown.onValueChanged.AddListener(index => OnDropdownChanged(rightHandWearableDropdown, Wearable.SlotEnum.Hand));
        leftHandWeaponDropdown.onValueChanged.AddListener(index => OnWeaponDropdownChanged(leftHandWeaponDropdown, leftHandWearableDropdown));
        leftHandWearableDropdown.onValueChanged.AddListener(index => OnDropdownChanged(leftHandWearableDropdown, Wearable.SlotEnum.Hand));
        petDropdown.onValueChanged.AddListener(index => OnDropdownChanged(petDropdown, Wearable.SlotEnum.Pet));
    }

    private void SetDropdownValues()
    {
        FindLatestPlayerEquipment();

        SetDropdownValue(bodyDropdown, playerEquipment.Body.Value);
        SetDropdownValue(faceDropdown, playerEquipment.Face.Value);
        SetDropdownValue(eyesDropdown, playerEquipment.Eyes.Value);
        SetDropdownValue(headDropdown, playerEquipment.Head.Value);
        SetDropdownValue(petDropdown, playerEquipment.Pet.Value);

        var rightHandWeaponType = WearableManager.Instance.GetWearable(playerEquipment.RightHand.Value).WeaponType;
        SetDropdownValue(rightHandWeaponDropdown, rightHandWeaponType);
        InitializeDropdown(rightHandWearableDropdown, Wearable.SlotEnum.Hand, rightHandWeaponType);
        SetDropdownValue(rightHandWearableDropdown, playerEquipment.RightHand.Value);

        var leftHandWeaponType = WearableManager.Instance.GetWearable(playerEquipment.LeftHand.Value).WeaponType;
        SetDropdownValue(leftHandWeaponDropdown, leftHandWeaponType);
        InitializeDropdown(leftHandWearableDropdown, Wearable.SlotEnum.Hand, leftHandWeaponType);
        SetDropdownValue(leftHandWearableDropdown, playerEquipment.LeftHand.Value);
    }

    void FindLatestPlayerEquipment()
    {
        // get the most up to date playerEquipment object
        playerEquipment = null;
        var players = Game.Instance.playerControllers;
        foreach (var player in players)
        {

            var networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsLocalPlayer)
            {
                playerEquipment = player.GetComponent<PlayerEquipment>();
                break;
            }
        }
    }

    private void InitializeDropdown(TMP_Dropdown dropdown, Wearable.SlotEnum slot)
    {
        var options = WearableManager.Instance.wearablesByNameEnum.Values
            .Where(w => w.Slot == slot)
            .OrderBy(w => w.Rarity == Wearable.RarityEnum.NA ? int.MaxValue : (int)w.Rarity) // Sort by rarity, move NA to the end
            .Select(w => w.NameType.ToString())
            .ToList();

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }


    private void InitializeDropdown(TMP_Dropdown dropdown, Wearable.SlotEnum slot, Wearable.WeaponTypeEnum weaponType)
    {
        var options = WearableManager.Instance.wearablesByNameEnum.Values
            .Where(w => w.Slot == slot && w.WeaponType == weaponType)
            .OrderBy(w => w.Rarity == Wearable.RarityEnum.NA ? int.MaxValue : (int)w.Rarity) // Sort by rarity, move NA to the end
            .Select(w => w.NameType.ToString())
            .ToList();

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }


    private void InitializeWeaponTypeDropdown(TMP_Dropdown dropdown)
    {
        var excludedTypes = new[]
        {
        Wearable.WeaponTypeEnum.Aura,
        Wearable.WeaponTypeEnum.Consume,
        Wearable.WeaponTypeEnum.Throw,
        Wearable.WeaponTypeEnum.NA
    };

        var options = System.Enum.GetValues(typeof(Wearable.WeaponTypeEnum))
            .Cast<Wearable.WeaponTypeEnum>()
            .Where(w => !excludedTypes.Contains(w)) // Exclude specified types
            .OrderBy(w => w.ToString()) // Sort alphabetically
            .Select(w => w.ToString())
            .ToList();

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    private void OnDropdownChanged(TMP_Dropdown dropdown, Wearable.SlotEnum slot)
    {
        FindLatestPlayerEquipment();

        var selectedNameEnum = (Wearable.NameEnum)System.Enum.Parse(typeof(Wearable.NameEnum), dropdown.options[dropdown.value].text);
        var wearable = WearableManager.Instance.GetWearable(selectedNameEnum);

        switch (slot)
        {
            case Wearable.SlotEnum.Body:
                playerEquipment.SetEquipmentServerRpc(PlayerEquipment.Slot.Body, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Face:
                playerEquipment.SetEquipmentServerRpc(PlayerEquipment.Slot.Face, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Eyes:
                playerEquipment.SetEquipmentServerRpc(PlayerEquipment.Slot.Eyes, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Head:
                playerEquipment.SetEquipmentServerRpc(PlayerEquipment.Slot.Head, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Hand:
                // Handle both left and right hand separately
                if (dropdown == rightHandWearableDropdown)
                {
                    playerEquipment.SetPlayerWeapon(Hand.Right, selectedNameEnum);
                }
                else if (dropdown == leftHandWearableDropdown)
                {
                    playerEquipment.SetPlayerWeapon(Hand.Left, selectedNameEnum);
                }
                break;
            case Wearable.SlotEnum.Pet:
                Debug.Log(selectedNameEnum);
                playerEquipment.SetEquipmentServerRpc(PlayerEquipment.Slot.Pet, selectedNameEnum);
                break;
        }
    }

    private void OnWeaponDropdownChanged(TMP_Dropdown weaponDropdown, TMP_Dropdown wearableDropdown)
    {
        var selectedWeaponType = (Wearable.WeaponTypeEnum)System.Enum.Parse(typeof(Wearable.WeaponTypeEnum), weaponDropdown.options[weaponDropdown.value].text);
        InitializeDropdown(wearableDropdown, Wearable.SlotEnum.Hand, selectedWeaponType);
        wearableDropdown.value = 0; // Reset to the first option
        OnDropdownChanged(wearableDropdown, Wearable.SlotEnum.Hand); // Update the wearable based on the new dropdown
    }

    private void SetDropdownValue(TMP_Dropdown dropdown, Wearable.NameEnum nameEnum)
    {
        dropdown.value = dropdown.options.FindIndex(option => option.text == nameEnum.ToString());
    }

    private void SetDropdownValue(TMP_Dropdown dropdown, Wearable.WeaponTypeEnum weaponType)
    {
        dropdown.value = dropdown.options.FindIndex(option => option.text == weaponType.ToString());
    }
}
