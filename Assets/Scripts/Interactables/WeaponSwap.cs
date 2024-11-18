using Unity.Netcode;
using UnityEngine;

public class WeaponSwap : Interactable
{
    public Wearable.NameEnum WeaponEnum;
    [HideInInspector] public NetworkVariable<Wearable.NameEnum> SyncNameEnum;
    public SpriteRenderer SpriteRenderer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Init(WeaponEnum);

        SyncNameEnum.Value = WeaponEnum;

        SyncNameEnum.OnValueChanged += OnNetworkNameChanged;
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        WeaponEnum = SyncNameEnum.Value;

        if (WeaponSwapCanvas_v2.Instance.isCanvasOpen)
        {
            WeaponSwapCanvas_v2.Instance.HideCanvas();
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
        }
        else
        {
            WeaponSwapCanvas_v2.Instance.activeWeaponSwap = GetComponent<WeaponSwap>();
            WeaponSwapCanvas_v2.Instance.newWeaponNameEnum = WeaponEnum;
            WeaponSwapCanvas_v2.Instance.ShowCanvas();
            PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
        }
    }

    public override void OnTriggerEnter2DInteraction()
    {
        base.OnTriggerEnter2DInteraction();

        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
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