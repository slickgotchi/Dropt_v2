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
    public Wearable.NameEnum starterPet = Wearable.NameEnum.None;

    private PlayerGotchi m_playerGotchi;

    private int m_gotchiId = -1;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_playerGotchi = GetComponent<PlayerGotchi>();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    // this function called by PlayerController when gotchi changes
    public void Init(int gotchiId)
    {
        //Debug.Log("PlayerEquipment init for: " + gotchiId);
        SetPlayerWeaponsByGotchiId(gotchiId);
        SetPlayerPetByGotchiId(gotchiId);
    }

    private void Update()
    {
        // weapon sprite updates
        UpdateWeaponSprites();

        // pet updates
        UpdatePets();
    }

    private void UpdateWeaponSprites()
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
    }

    private void UpdatePets()
    {
        if (IsServer)
        {
            if (m_localPet != Pet.Value)
            {
                // see if player owns a pet already
                var petControllers = Game.Instance.petControllers;
                for (int i = 0; i < petControllers.Count; i++)
                {
                    var petController = petControllers[i];
                    if (petController.GetPlayerNetworkObjectId() == GetComponent<NetworkObject>().NetworkObjectId)
                    {
                        // we have a match, we need to despawn the old pet
                        petController.GetComponent<NetworkObject>().Despawn();
                    }
                }


                // create a new pet
                PetType myPet;
                if (System.Enum.TryParse(Pet.Value.ToString(), out myPet))
                {
                    PetsManager.Instance.SpawnPet(myPet, transform.position, GetComponent<NetworkObject>().NetworkObjectId);
                }

                // set new local pet
                m_localPet = Pet.Value;
            }
        }
    }

    public void SetPlayerWeaponsByGotchiId(int id)
    {
        if (!IsLocalPlayer) return;



        // get gotchi data
        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
        if (gotchiData != null)
        {
            var lhWearable = WearableManager.Instance.GetWearable(gotchiData.equippedWearables[5]);
            var rhWearable = WearableManager.Instance.GetWearable(gotchiData.equippedWearables[4]);

            SetEquipmentServerRpc(Slot.LeftHand, lhWearable != null ? lhWearable.NameType : Wearable.NameEnum.Unarmed);
            SetEquipmentServerRpc(Slot.RightHand, rhWearable != null ? rhWearable.NameType : Wearable.NameEnum.Unarmed);

            return;
        }

        // try get offchain gotchi data
        var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(id);
        if (offchainGotchiData != null)
        {
            var lhWearable = WearableManager.Instance.GetWearable(offchainGotchiData.equippedWearables[5]);
            var rhWearable = WearableManager.Instance.GetWearable(offchainGotchiData.equippedWearables[4]);

            var lh = lhWearable != null ? lhWearable.NameType : Wearable.NameEnum.Unarmed;
            var rh = rhWearable != null ? rhWearable.NameType : Wearable.NameEnum.Unarmed;

            SetEquipmentServerRpc(Slot.LeftHand, lhWearable != null ? lhWearable.NameType : Wearable.NameEnum.Unarmed);
            SetEquipmentServerRpc(Slot.RightHand, rhWearable != null ? rhWearable.NameType : Wearable.NameEnum.Unarmed);

            return;
        }

        Debug.LogWarning("No gotchi data found for: " + id);

    }

    public void SetPlayerPetByGotchiId(int id)
    {
        if (!IsLocalPlayer) return;

        // get gotchi data
        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
        if (gotchiData != null)
        {
            var petWearable = WearableManager.Instance.GetWearable(gotchiData.equippedWearables[6]);
            SetEquipmentServerRpc(Slot.Pet, petWearable != null ? petWearable.NameType : Wearable.NameEnum.Null);
            return;
        }

        // try get offchain gotchi data
        var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(id);
        if (offchainGotchiData != null)
        {
            var petWearable = WearableManager.Instance.GetWearable(offchainGotchiData.equippedWearables[6]);
            SetEquipmentServerRpc(Slot.Pet, petWearable != null ? petWearable.NameType : Wearable.NameEnum.Null);
            return;
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
        if (wearable != null)
        {
            GetComponent<PlayerCharacter>().SetWearableBuffServerRpc(slot, wearable.Id);
            CheckWeaponIsShieldBlock(slot, wearable);
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
                    shieldBlock.Initialize(Hand.Left, wearable.Rarity);
                    ShowPlayerHudClientRpc(Hand.Left, shieldBlock.GetHpRatio(Hand.Left));
                }
                else if (slot == Slot.RightHand)
                {
                    shieldBlock.Initialize(Hand.Right, wearable.Rarity);
                    ShowPlayerHudClientRpc(Hand.Right, shieldBlock.GetHpRatio(Hand.Right));
                }
                break;
            default:
                if (slot == Slot.LeftHand)
                {
                    shieldBlock.Deactivate(Hand.Left);
                    HidePlayerHudClientRpc(Hand.Left);
                }
                else if (slot == Slot.RightHand)
                {
                    shieldBlock.Deactivate(Hand.Right);
                    HidePlayerHudClientRpc(Hand.Right);
                }
                break;
        }
    }

    [ClientRpc]
    private void ShowPlayerHudClientRpc(Hand hand, float progress)
    {
        if (!IsLocalPlayer) return;

        PlayerHUDCanvas.Instance.SetShieldBarProgress(hand, progress);
        PlayerHUDCanvas.Instance.VisibleShieldBar(hand, true);
    }

    [ClientRpc]
    private void HidePlayerHudClientRpc(Hand hand)
    {
        if (!IsLocalPlayer) return;

        PlayerHUDCanvas.Instance.VisibleShieldBar(hand, false);
    }

    public enum Slot
    {
        Body, Face, Eyes, Head, RightHand, LeftHand, Pet,
    }
}
