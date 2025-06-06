using Unity.Netcode;
using UnityEngine;
using Unity.Mathematics;

public class WeaponSwap : Interactable
{
    public Wearable.NameEnum WeaponEnum;
    [HideInInspector] public NetworkVariable<Wearable.NameEnum> SyncNameEnum;
    public SpriteRenderer SpriteRenderer;

    private Wearable.NameEnum m_localWeaponName = Wearable.NameEnum._10GallonHat;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Init(WeaponEnum);

        if (IsServer) SyncNameEnum.Value = WeaponEnum;
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        // we are closest so we can try swap out weapon
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

        WeaponSwapCanvas_v2.Instance.interactable = this;
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);

        WeaponSwapCanvas_v2.Instance.interactable = null;
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

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (IsClient && m_localWeaponName != SyncNameEnum.Value)
        {
            m_localWeaponName = SyncNameEnum.Value;
            Init(m_localWeaponName);
        }
    }

    private void Init(Wearable.NameEnum wearableNameEnum)
    {
        var sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, PlayerGotchi.Facing.Right);
        SpriteRenderer.sprite = sprite;
    }
}