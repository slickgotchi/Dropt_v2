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

    private PlayerGotchi m_playerGotchi;

    private void Awake()
    {
        m_playerGotchi = GetComponent<PlayerGotchi>();
    }

    public override void OnNetworkSpawn()
    {
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
        // right hand changes
        if (RightHand.Value != m_localRightHand)
        {
            m_localRightHand = RightHand.Value;
            m_playerGotchi.SetWeaponSprites(Hand.Right, m_localRightHand);
        }

        // left hand changes
        if (LeftHand.Value != m_localLeftHand)
        {
            m_localLeftHand = LeftHand.Value;
            m_playerGotchi.SetWeaponSprites(Hand.Left, m_localLeftHand);
        }
    }

    public void SetPlayerWeaponsByGotchiId(int id)
    {
        if (!IsClient) return;

        if (id <= 0)
        {
            //m_playerGotchi.SetWeaponSprites(Hand.Left, Wearable.NameEnum.Unarmed);
            //m_playerGotchi.SetWeaponSprites(Hand.Right, Wearable.NameEnum.Unarmed);

            // tell server to change our equipment if we're the local player
            if (IsLocalPlayer)
            {
                SetEquipmentServerRpc(Slot.LeftHand, Wearable.NameEnum.Unarmed);
                SetEquipmentServerRpc(Slot.RightHand, Wearable.NameEnum.Unarmed);
            }
        } else
        {
            var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(id);
            var rightHandWearableId = gotchiData.equippedWearables[4];
            var leftHandWearableId = gotchiData.equippedWearables[5];

            var lhWearable = WearableManager.Instance.GetWearable(leftHandWearableId);
            var rhWearable = WearableManager.Instance.GetWearable(rightHandWearableId);

            //m_playerGotchi.SetWeaponSprites(Hand.Left, lhWearable.NameType);
            //m_playerGotchi.SetWeaponSprites(Hand.Right, rhWearable.NameType);

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
    }

    public enum Slot
    {
        Body, Face, Eyes, Head, RightHand, LeftHand, Pet,
    }
}
