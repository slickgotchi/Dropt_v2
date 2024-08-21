using Unity.Netcode;
using UnityEngine;

public class WeaponSwap : Interactable
{
    public NetworkVariable<Wearable.NameEnum> SyncNameEnum;
    public SpriteRenderer SpriteRenderer;

    GameObject m_player;
    
    public override void OnStartInteraction()
    {
        WeaponSwapCanvas.Instance.Container.SetActive(true);
        WeaponSwapCanvas.Instance.Init(GetComponent<WeaponSwap>().SyncNameEnum.Value);
        m_player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
    }

    public override void OnUpdateInteraction()
    {
        var wearableNameEnum = GetComponent<WeaponSwap>().SyncNameEnum.Value;
        if (Input.GetMouseButtonDown(0))
        {
            var ogEquipment = m_player.GetComponent<PlayerEquipment>().LeftHand.Value;
<<<<<<< HEAD
            m_player.GetComponent<PlayerEquipment>().SetEquipmentServerRpc(PlayerEquipment.Slot.LeftHand, wearableNameEnum);
            GetComponent<WeaponSwap>().Init(ogEquipment);
=======
            m_player.GetComponent<PlayerEquipment>().SetEquipment(PlayerEquipment.Slot.LeftHand, wearableNameEnum);
            SendWearableNameRpc(ogEquipment);
>>>>>>> 65f8b13b ([CHG] finalize chests logic)
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ogEquipment = m_player.GetComponent<PlayerEquipment>().RightHand.Value;
<<<<<<< HEAD
            m_player.GetComponent<PlayerEquipment>().SetEquipmentServerRpc(PlayerEquipment.Slot.RightHand, wearableNameEnum);
            GetComponent<WeaponSwap>().Init(ogEquipment);
=======
            m_player.GetComponent<PlayerEquipment>().SetEquipment(PlayerEquipment.Slot.RightHand, wearableNameEnum);
            SendWearableNameRpc(ogEquipment);
>>>>>>> 65f8b13b ([CHG] finalize chests logic)
        }
    }

    public override void OnFinishInteraction()
    {
        WeaponSwapCanvas.Instance.Container.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Init(SyncNameEnum.Value);

        SyncNameEnum.OnValueChanged += OnNetworkNameChanged;
    }

    [Rpc(SendTo.Server)]
    private void SendWearableNameRpc(Wearable.NameEnum ogEquipment)
    {
        if (!IsServer)
        {
            return;
        }

        SyncNameEnum.Value = ogEquipment;
    }

    private void OnNetworkNameChanged(Wearable.NameEnum prevValue, Wearable.NameEnum newValue)
    {
        Init(newValue);
    }

    private void Init(Wearable.NameEnum wearableNameEnum)
    {
        var sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, PlayerGotchi.Facing.Right);
        SpriteRenderer.sprite = sprite;
    }
}