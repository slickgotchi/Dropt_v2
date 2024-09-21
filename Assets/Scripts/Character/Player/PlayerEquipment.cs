using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using GotchiHub;

public class PlayerEquipment : NetworkBehaviour
{
    public NetworkVariable<Wearable.NameEnum> Body = new NetworkVariable<Wearable.NameEnum>(Wearable.NameEnum.Null);
    public NetworkVariable<Wearable.NameEnum> Face = new NetworkVariable<Wearable.NameEnum>(Wearable.NameEnum.Null);
    public NetworkVariable<Wearable.NameEnum> Eyes = new NetworkVariable<Wearable.NameEnum>(Wearable.NameEnum.Null);
    public NetworkVariable<Wearable.NameEnum> Head = new NetworkVariable<Wearable.NameEnum>(Wearable.NameEnum.Null);
    public NetworkVariable<Wearable.NameEnum> RightHand = new NetworkVariable<Wearable.NameEnum>(Wearable.NameEnum.Null);
    public NetworkVariable<Wearable.NameEnum> LeftHand = new NetworkVariable<Wearable.NameEnum>(Wearable.NameEnum.Null);
    public NetworkVariable<Wearable.NameEnum> Pet = new NetworkVariable<Wearable.NameEnum>(Wearable.NameEnum.Null);

    private Wearable.NameEnum m_localBody = Wearable.NameEnum.Null;
    private Wearable.NameEnum m_localFace = Wearable.NameEnum.Null;
    private Wearable.NameEnum m_localEyes = Wearable.NameEnum.Null;
    private Wearable.NameEnum m_localHead = Wearable.NameEnum.Null;
    private Wearable.NameEnum m_localRightHand = Wearable.NameEnum.Null;
    private Wearable.NameEnum m_localLeftHand = Wearable.NameEnum.Null;
    private Wearable.NameEnum m_localPet = Wearable.NameEnum.Null;

    public Wearable.NameEnum leftHandStarterWeapon = Wearable.NameEnum.Unarmed;
    public Wearable.NameEnum rightHandStarterWeapon = Wearable.NameEnum.Unarmed;

    private PlayerGotchi m_playerGotchi;

    private void Awake()
    {
        m_playerGotchi = GetComponent<PlayerGotchi>();
    }

    public override void OnNetworkSpawn()
    {
        // starter weapons get set in Init()
    }

    public override void OnNetworkDespawn()
    {
    }

    public void Init(int gotchiId)
    {
        if (!IsClient) return;

        SetPlayerWeaponsByGotchiId(gotchiId);
    }

    private void Update()
    {
        // right hand sprite changes
        if (RightHand.Value != m_localRightHand)
        {
            m_localRightHand = RightHand.Value;
            m_playerGotchi.SetWeaponSprites(Hand.Right, m_localRightHand);
        }

        // left hand sprite changes
        if (LeftHand.Value != m_localLeftHand)
        {
            m_localLeftHand = LeftHand.Value;
            m_playerGotchi.SetWeaponSprites(Hand.Left, m_localLeftHand);
        }

        // pet updates
        UpdatePets();
    }

    private void UpdatePets()
    {
        if (!IsServer) return;
        if (m_localPet == Pet.Value) return;

        // see if player owns a pet already
        var petControllers = FindObjectsByType<PetController>(FindObjectsSortMode.None);
        for (int i = 0; i < petControllers.Length; i++)
        {
            var petController = petControllers[i];
            if (petController.OwnerClientId == GetComponent<NetworkObject>().NetworkObjectId)
            {
                // we have a match, we need to despawn the old pet
                petController.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    public void SetPlayerWeaponsByGotchiId(int id)
    {
        if (!IsClient) return;

        if (id <= 0)
        {
            // tell server to change our equipment if we're the local player
            if (IsLocalPlayer)
            {
                SetEquipmentServerRpc(Slot.LeftHand, leftHandStarterWeapon);
                SetEquipmentServerRpc(Slot.RightHand, rightHandStarterWeapon);
            }
        }
        else
        {
            var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
            var rightHandWearableId = gotchiData.equippedWearables[4];
            var leftHandWearableId = gotchiData.equippedWearables[5];

            var lhWearable = WearableManager.Instance.GetWearable(leftHandWearableId);
            var rhWearable = WearableManager.Instance.GetWearable(rightHandWearableId);

            // tell server to change our equipment if we're the local player
            if (IsLocalPlayer)
            {
                SetEquipmentServerRpc(Slot.LeftHand, lhWearable != null ? lhWearable.NameType : Wearable.NameEnum.Unarmed);
                SetEquipmentServerRpc(Slot.RightHand, rhWearable != null ? rhWearable.NameType : Wearable.NameEnum.Unarmed);
            }
        }

    }

    public void SetPlayerWeapon(Hand hand, Wearable.NameEnum nameEnum)
    {
        m_playerGotchi.SetWeaponSprites(hand, nameEnum);

        if (IsLocalPlayer)
        {
            SetEquipmentServerRpc(hand == Hand.Left ? Slot.LeftHand : Slot.RightHand, nameEnum);
        }
    }

    [Rpc(SendTo.Server)]
    public void SetEquipmentServerRpc(Slot slot, Wearable.NameEnum equipmentNameEnum)
    {
        switch (slot)
        {
            case Slot.Body: Body.Value = equipmentNameEnum; break;
            case Slot.Face: Face.Value = equipmentNameEnum; break;
            case Slot.Eyes: Eyes.Value = equipmentNameEnum; break;
            case Slot.Head: Head.Value = equipmentNameEnum; break;
            case Slot.RightHand: RightHand.Value = equipmentNameEnum; break;
            case Slot.LeftHand: LeftHand.Value = equipmentNameEnum; break;
            case Slot.Pet: Pet.Value = equipmentNameEnum; break;
            default: break;
        }

        var wearable = WearableManager.Instance.GetWearable(equipmentNameEnum);
        GetComponent<PlayerCharacter>().SetWearableBuffServerRpc(slot, wearable.Id);
        CheckWeaponIsShieldBlock(slot, wearable);

        // if slot was pet, we should spawn a pet
        if (slot == Slot.Pet)
        {
            PetType myPet;
            if (System.Enum.TryParse(equipmentNameEnum.ToString(), out myPet))
            {
                // destroy any old pets
                var playerObjectId = GetComponent<NetworkObject>().NetworkObjectId;
                var allPets = FindObjectsByType<PetController>(FindObjectsSortMode.None);
                foreach (var pet in allPets)
                {
                    if (pet.GetPlayerNetworkObjectId() == playerObjectId)
                    {
                        pet.GetComponent<NetworkObject>().Despawn();
                    }
                }

                PetsManager.Instance.SpawnPet(myPet, transform.position, GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    private void CheckWeaponIsShieldBlock(Slot slot, Wearable wearable)
    {
        PlayerAbilityEnum ability = GetComponent<PlayerAbilities>().GetHoldAbilityEnum(wearable.NameType);
        ShieldBlock shieldBlock = GetComponentInChildren<ShieldBlock>();
        switch (ability)
        {
            case PlayerAbilityEnum.ShieldBlock:
                if (slot == Slot.LeftHand)
                {
                    shieldBlock.Initialize(wearable.NameType, Hand.Left, wearable.Rarity);
                    PlayerHUDCanvas.Singleton.SetShieldBarProgress(Hand.Left, shieldBlock.GetHpRatio());
                    PlayerHUDCanvas.Singleton.VisibleShieldBar(Hand.Left, true);
                }
                else if (slot == Slot.RightHand)
                {
                    shieldBlock.Initialize(wearable.NameType, Hand.Right, wearable.Rarity);
                    PlayerHUDCanvas.Singleton.SetShieldBarProgress(Hand.Right, shieldBlock.GetHpRatio());
                    PlayerHUDCanvas.Singleton.VisibleShieldBar(Hand.Right, true);
                }
                break;
            default:
                if (slot == Slot.LeftHand)
                {
                    shieldBlock.Deactivate(Hand.Left);
                    PlayerHUDCanvas.Singleton.VisibleShieldBar(Hand.Left, false);
                }
                else if (slot == Slot.RightHand)
                {
                    shieldBlock.Deactivate(Hand.Right);
                    PlayerHUDCanvas.Singleton.VisibleShieldBar(Hand.Right, false);
                }
                break;
        }
    }

    public enum Slot
    {
        Body, Face, Eyes, Head, RightHand, LeftHand, Pet,
    }
}
