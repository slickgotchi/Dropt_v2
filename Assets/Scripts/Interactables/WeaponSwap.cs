using Unity.Netcode;
using UnityEngine;

public class WeaponSwap : Interactable
{
    public Wearable.NameEnum WeaponEnum;
    [HideInInspector] public NetworkVariable<Wearable.NameEnum> SyncNameEnum;
    public SpriteRenderer SpriteRenderer;

    //GameObject m_player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Init(WeaponEnum);

        SyncNameEnum.Value = WeaponEnum;

        SyncNameEnum.OnValueChanged += OnNetworkNameChanged;
    }

    public override void OnPressOpenInteraction()
    {
        base.OnPressOpenInteraction();

        WeaponEnum = SyncNameEnum.Value;

        WeaponSwapCanvas_v2.Instance.activeWeaponSwap = GetComponent<WeaponSwap>();
        WeaponSwapCanvas_v2.Instance.newWeaponNameEnum = WeaponEnum;
        WeaponSwapCanvas_v2.Instance.ShowCanvas();

        //WeaponSwapCanvas.Instance.Container.SetActive(true);
        //UpdateCanvas();
    }

    public override void OnPressCloseInteraction()
    {
        WeaponSwapCanvas_v2.Instance.HideCanvas();

        base.OnPressCloseInteraction();


        //WeaponSwapCanvas.Instance.Container.SetActive(false);
    }

    //public override void OnTriggerStartInteraction()
    //{
    //    //WeaponSwapCanvas.Instance.Container.SetActive(true);
    //    //UpdateCanvas();
    //    //m_player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
    //}

    //public override void OnTriggerUpdateInteraction()
    //{
    //    //var wearableNameEnum = SyncNameEnum.Value;

    //    //if (Input.GetMouseButtonDown(0))
    //    //{
    //    //    var ogEquipment = m_player.GetComponent<PlayerEquipment>().LeftHand.Value;

    //    //    m_player.GetComponent<PlayerEquipment>()
    //    //        .SetEquipmentServerRpc(PlayerEquipment.Slot.LeftHand, wearableNameEnum);

    //    //    SetWearableNameRpc(ogEquipment);

    //    //    return;
    //    //}

    //    //if (Input.GetMouseButtonDown(1))
    //    //{
    //    //    var ogEquipment = m_player.GetComponent<PlayerEquipment>().RightHand.Value;

    //    //    m_player.GetComponent<PlayerEquipment>()
    //    //        .SetEquipmentServerRpc(PlayerEquipment.Slot.RightHand, wearableNameEnum);

    //    //    SetWearableNameRpc(ogEquipment);
    //    //}
    //}

    public void SwapWeapon(Hand hand, Wearable.NameEnum wearableNameEnum, PlayerController localPlayer)
    {
        //m_player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;

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

    //public override void OnTriggerFinishInteraction()
    //{
    //    //WeaponSwapCanvas.Instance.Container.SetActive(false);
    //}

    //private void UpdateCanvas()
    //{
    //    //WeaponSwapCanvas.Instance.Init(SyncNameEnum.Value);
    //}



    private void OnNetworkNameChanged(Wearable.NameEnum prevValue, Wearable.NameEnum newValue)
    {
        Init(newValue);
    }

    private void Init(Wearable.NameEnum wearableNameEnum)
    {
        var sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, PlayerGotchi.Facing.Right);
        SpriteRenderer.sprite = sprite;
        //UpdateCanvas();
    }
}