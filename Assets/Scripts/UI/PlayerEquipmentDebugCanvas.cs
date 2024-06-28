using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerEquipmentDebugCanvas : MonoBehaviour
{
    public static PlayerEquipmentDebugCanvas Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Container.SetActive(false);
    }

    private PlayerEquipment playerEquipment;

    public GameObject Container;

    public TMP_Dropdown bodyDropdown;
    public TMP_Dropdown faceDropdown;
    public TMP_Dropdown eyesDropdown;
    public TMP_Dropdown headDropdown;
    public TMP_Dropdown rightHandWeaponDropdown;
    public TMP_Dropdown rightHandWearableDropdown;
    public TMP_Dropdown leftHandWeaponDropdown;
    public TMP_Dropdown leftHandWearableDropdown;
    public TMP_Dropdown petDropdown;

    public static bool IsActive()
    {
        return Instance.Container.activeSelf;
    }

    private void Update()
    {
        if (playerEquipment == null)
        {
            // Find the local player's PlayerEquipment component
            var players = FindObjectsByType<PlayerEquipment>(FindObjectsSortMode.None);

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<NetworkObject>().IsLocalPlayer)
                {
                    playerEquipment = players[i];
                    InitializeDropdowns();
                    SetUpDropdownListeners();
                    SetDropdownValues();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Container.SetActive(!Container.activeSelf);
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

    private void InitializeDropdown(TMP_Dropdown dropdown, Wearable.SlotEnum slot)
    {
        var options = WearableManager.Instance.wearablesByNameEnum.Values
            .Where(w => w.Slot == slot)
            .Select(w => w.NameType.ToString())
            .ToList();

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    private void InitializeDropdown(TMP_Dropdown dropdown, Wearable.SlotEnum slot, Wearable.WeaponTypeEnum weaponType)
    {
        var options = WearableManager.Instance.wearablesByNameEnum.Values
            .Where(w => w.Slot == slot && w.WeaponType == weaponType)
            .Select(w => w.NameType.ToString())
            .ToList();

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    private void InitializeWeaponTypeDropdown(TMP_Dropdown dropdown)
    {
        var options = System.Enum.GetNames(typeof(Wearable.WeaponTypeEnum)).ToList();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    private void OnDropdownChanged(TMP_Dropdown dropdown, Wearable.SlotEnum slot)
    {
        var selectedNameEnum = (Wearable.NameEnum)System.Enum.Parse(typeof(Wearable.NameEnum), dropdown.options[dropdown.value].text);
        var wearable = WearableManager.Instance.GetWearable(selectedNameEnum);

        switch (slot)
        {
            case Wearable.SlotEnum.Body:
                playerEquipment.SetEquipment(PlayerEquipment.Slot.Body, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Face:
                playerEquipment.SetEquipment(PlayerEquipment.Slot.Face, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Eyes:
                playerEquipment.SetEquipment(PlayerEquipment.Slot.Eyes, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Head:
                playerEquipment.SetEquipment(PlayerEquipment.Slot.Head, selectedNameEnum);
                break;
            case Wearable.SlotEnum.Hand:
                // Handle both left and right hand separately
                if (dropdown == rightHandWearableDropdown)
                {
                    playerEquipment.SetEquipment(PlayerEquipment.Slot.RightHand, selectedNameEnum);
                }
                else if (dropdown == leftHandWearableDropdown)
                {
                    playerEquipment.SetEquipment(PlayerEquipment.Slot.LeftHand, selectedNameEnum);
                }
                break;
            case Wearable.SlotEnum.Pet:
                playerEquipment.SetEquipment(PlayerEquipment.Slot.Pet, selectedNameEnum);
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
