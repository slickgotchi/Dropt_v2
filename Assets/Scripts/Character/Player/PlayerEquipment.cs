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

    private PlayerGotchi m_playerGotchi;

    private void Awake()
    {
        m_playerGotchi = GetComponent<PlayerGotchi>();
    }

    public override void OnNetworkSpawn()
    {
        RightHand.OnValueChanged += OnRightHandChanged;
        LeftHand.OnValueChanged += OnLeftHandChanged;
    }

    public override void OnNetworkDespawn()
    {
        RightHand.OnValueChanged -= OnRightHandChanged;
        LeftHand.OnValueChanged -= OnLeftHandChanged;
    }

    void OnRightHandChanged(Wearable.NameEnum prev, Wearable.NameEnum curr)
    {
        if (IsClient) m_playerGotchi.SetWeaponSprites(Hand.Right, curr);
    }

    void OnLeftHandChanged(Wearable.NameEnum prev, Wearable.NameEnum curr)
    {
        if (IsClient) m_playerGotchi.SetWeaponSprites(Hand.Left, curr);
    }

    public void SetPlayerEquipmentByGotchiId(int id)
    {
        if (!IsClient) return;

        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
        var rightHandWearableId = gotchiData.equippedWearables[4];
        var leftHandWearableId = gotchiData.equippedWearables[5];

        var lhWearable = WearableManager.Instance.GetWearable(leftHandWearableId);
        var rhWearable = WearableManager.Instance.GetWearable(rightHandWearableId);

        // tell server to change our equipment
        SetEquipmentServerRpc(Slot.LeftHand, lhWearable != null ? lhWearable.NameType : Wearable.NameEnum.Unarmed);
        SetEquipmentServerRpc(Slot.RightHand, rhWearable != null ? rhWearable.NameType : Wearable.NameEnum.Unarmed);
    }

    public void Init(int gotchiId)
    {
        if (!IsClient) return;

        SetPlayerEquipmentByGotchiId(gotchiId);
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
    }

    public enum Slot
    {
        Body, Face, Eyes, Head, RightHand, LeftHand, Pet,
    }
}
