using Unity.Netcode;
using UnityEngine;

public class WeaponSwap : Interactable
{
    public Wearable.NameEnum WeaponEnum;
    [HideInInspector] public NetworkVariable<Wearable.NameEnum> SyncNameEnum;
    public SpriteRenderer SpriteRenderer;

    private bool m_isCanvasOpen;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Init(WeaponEnum);

        SyncNameEnum.Value = WeaponEnum;

        SyncNameEnum.OnValueChanged += OnNetworkNameChanged;

        m_isCanvasOpen = false;
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        WeaponEnum = SyncNameEnum.Value;

        if (m_isCanvasOpen)
        {
            WeaponSwapCanvas_v2.Instance.HideCanvas();
        }
        else
        {
            WeaponSwapCanvas_v2.Instance.activeWeaponSwap = GetComponent<WeaponSwap>();
            WeaponSwapCanvas_v2.Instance.newWeaponNameEnum = WeaponEnum;
            WeaponSwapCanvas_v2.Instance.ShowCanvas();
        }

        m_isCanvasOpen = !m_isCanvasOpen;
    }

    public void SwapWeapon(Hand hand, Wearable.NameEnum wearableNameEnum, PlayerController localPlayer)
    {
        if (hand == Hand.Left)
        {
            var ogEquipment = localPlayer.GetComponent<PlayerEquipment>().LeftHand.Value;

            localPlayer.GetComponent<PlayerEquipment>()
                .SetEquipmentServerRpc(PlayerEquipment.Slot.LeftHand, wearableNameEnum);

            SetWearableNameRpc(ogEquipment);
        }

        if (hand == Hand.Right)
        {
            var ogEquipment = localPlayer.GetComponent<PlayerEquipment>().RightHand.Value;

            localPlayer.GetComponent<PlayerEquipment>()
                .SetEquipmentServerRpc(PlayerEquipment.Slot.RightHand, wearableNameEnum);

            SetWearableNameRpc(ogEquipment);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetWearableNameRpc(Wearable.NameEnum ogEquipment)
    {
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