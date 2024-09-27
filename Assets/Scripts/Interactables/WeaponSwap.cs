using Unity.Netcode;
using UnityEngine;

public class WeaponSwap : Interactable
{
    public Wearable.NameEnum WeaponEnum;
    [HideInInspector] public NetworkVariable<Wearable.NameEnum> SyncNameEnum;
    public SpriteRenderer SpriteRenderer;

    GameObject m_player;

    public override void OnTriggerStartInteraction()
    {
        WeaponSwapCanvas.Instance.Container.SetActive(true);
        UpdateCanvas();
        m_player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
    }

    public override void OnTriggerUpdateInteraction()
    {
        var wearableNameEnum = SyncNameEnum.Value;

        if (Input.GetMouseButtonDown(0))
        {
            var ogEquipment = m_player.GetComponent<PlayerEquipment>().LeftHand.Value;

            m_player.GetComponent<PlayerEquipment>()
                .SetEquipmentServerRpc(PlayerEquipment.Slot.LeftHand, wearableNameEnum);

            SetWearableNameRpc(ogEquipment);

            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            var ogEquipment = m_player.GetComponent<PlayerEquipment>().RightHand.Value;

            m_player.GetComponent<PlayerEquipment>()
                .SetEquipmentServerRpc(PlayerEquipment.Slot.RightHand, wearableNameEnum);

            SetWearableNameRpc(ogEquipment);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetWearableNameRpc(Wearable.NameEnum ogEquipment)
    {
        SyncNameEnum.Value = ogEquipment;
    }

    public override void OnTriggerFinishInteraction()
    {
        WeaponSwapCanvas.Instance.Container.SetActive(false);
    }

    private void UpdateCanvas()
    {
        WeaponSwapCanvas.Instance.Init(SyncNameEnum.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Init(WeaponEnum);

        SyncNameEnum.Value = WeaponEnum;

        SyncNameEnum.OnValueChanged += OnNetworkNameChanged;
    }

    private void OnNetworkNameChanged(Wearable.NameEnum prevValue, Wearable.NameEnum newValue)
    {
        Init(newValue);
    }

    private void Init(Wearable.NameEnum wearableNameEnum)
    {
        var sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, PlayerGotchi.Facing.Right);
        SpriteRenderer.sprite = sprite;
        UpdateCanvas();
    }
}