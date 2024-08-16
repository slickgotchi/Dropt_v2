using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using GotchiHub;

public class PlayerEquipment : NetworkBehaviour
{
    public NetworkVariable<Wearable.NameEnum> Body;
    public NetworkVariable<Wearable.NameEnum> Face;
    public NetworkVariable<Wearable.NameEnum> Eyes;
    public NetworkVariable<Wearable.NameEnum> Head;
    public NetworkVariable<Wearable.NameEnum> RightHand;
    public NetworkVariable<Wearable.NameEnum> LeftHand;
    public NetworkVariable<Wearable.NameEnum> Pet;

    private void Awake()
    {
        GotchiDataManager.Instance.onSelectedGotchi += HandleOnSelectedGotchi;
    }

    void HandleOnSelectedGotchi(int id)
    {
        if (!IsClient) return;

        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
        var rightHandWearableId = gotchiData.equippedWearables[4];
        var leftHandWearableId = gotchiData.equippedWearables[5];

        var lhWearable = WearableManager.Instance.GetWearable(leftHandWearableId);
        var rhWearable = WearableManager.Instance.GetWearable(rightHandWearableId);

        SetEquipment(Slot.LeftHand, lhWearable != null ? lhWearable.NameType : Wearable.NameEnum.Unarmed);
        SetEquipment(Slot.RightHand, rhWearable != null ? rhWearable.NameType : Wearable.NameEnum.Unarmed);
    }

    public override void OnNetworkSpawn()
    {
        LeftHand.Value = Wearable.NameEnum.OliversSpoon;
        RightHand.Value = Wearable.NameEnum.Pitchfork;

        SetEquipment(Slot.LeftHand, LeftHand.Value);
        SetEquipment(Slot.RightHand, RightHand.Value);  
    }

    public void SetEquipment(Slot slot, Wearable.NameEnum equipmentNameEnum)
    {
        SetEquipmentServerRpc(slot, equipmentNameEnum);

        if (IsLocalPlayer)
        {
            if (slot == Slot.LeftHand)
            {
                GetComponent<PlayerGotchi>().SetWeaponSprites(Hand.Left, equipmentNameEnum);
            } 
            else if (slot == Slot.RightHand)
            {
                GetComponent<PlayerGotchi>().SetWeaponSprites(Hand.Right, equipmentNameEnum);
            }
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

        SetEquipmentClientRpc(slot, equipmentNameEnum);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetEquipmentClientRpc(Slot slot, Wearable.NameEnum equipmentNameEnum)
    {
        if (!IsLocalPlayer)
        {
            if (slot == Slot.LeftHand)
            {
                GetComponent<PlayerGotchi>().SetWeaponSprites(Hand.Left, equipmentNameEnum);
            }
            else if (slot == Slot.RightHand)
            {
                GetComponent<PlayerGotchi>().SetWeaponSprites(Hand.Right, equipmentNameEnum);
            }
        }
    }

    public enum Slot
    {
        Body, Face, Eyes, Head, RightHand, LeftHand, Pet,
    }
}
