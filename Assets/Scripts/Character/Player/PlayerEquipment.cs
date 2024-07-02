using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerEquipment : NetworkBehaviour
{
    public NetworkVariable<Wearable.NameEnum> Body;
    public NetworkVariable<Wearable.NameEnum> Face;
    public NetworkVariable<Wearable.NameEnum> Eyes;
    public NetworkVariable<Wearable.NameEnum> Head;
    public NetworkVariable<Wearable.NameEnum> RightHand;
    public NetworkVariable<Wearable.NameEnum> LeftHand;
    public NetworkVariable<Wearable.NameEnum> Pet;

    public override void OnNetworkSpawn()
    {
        LeftHand.Value = Wearable.NameEnum.MK2Grenade;
        RightHand.Value = Wearable.NameEnum.Basketball;
    }

    public void SetEquipment(Slot slot, Wearable.NameEnum equipmentNameEnum)
    {
        SetEquipmentServerRpc(slot, equipmentNameEnum);

        if (slot == Slot.LeftHand || slot == Slot.RightHand)
        {
            GetComponent<PlayerGotchi>().SetWeaponSprites(slot == Slot.LeftHand ? Hand.Left : Hand.Right, equipmentNameEnum);
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
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetEquipmentClientRpc(Slot slot, Wearable.NameEnum equipmentNameEnum)
    {
        if (!IsLocalPlayer)
        {
            GetComponent<PlayerGotchi>().SetWeaponSprites(slot == Slot.LeftHand ? Hand.Left : Hand.Right, equipmentNameEnum);
        }
    }

    public enum Slot
    {
        Body, Face, Eyes, Head, RightHand, LeftHand, Pet,
    }
}
