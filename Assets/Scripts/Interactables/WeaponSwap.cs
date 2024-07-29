using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponSwap : Interactable
{
    public Wearable.NameEnum WearableNameEnum;
    public SpriteRenderer SpriteRenderer;

    GameObject m_player;

    private void Awake()
    {
        Init(WearableNameEnum);
    }

    public override void OnStartInteraction()
    {
        WeaponSwapCanvas.Instance.Container.SetActive(true);
        WeaponSwapCanvas.Instance.Init(GetComponent<WeaponSwap>().WearableNameEnum);
        m_player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].gameObject;
    }

    public override void OnUpdateInteraction()
    {
        var wearableNameEnum = GetComponent<WeaponSwap>().WearableNameEnum;
        if (Input.GetMouseButtonDown(0))
        {
            var ogEquipment = m_player.GetComponent<PlayerEquipment>().LeftHand.Value;
            m_player.GetComponent<PlayerEquipment>().SetEquipment(PlayerEquipment.Slot.LeftHand, wearableNameEnum);
            GetComponent<WeaponSwap>().Init(ogEquipment);
        }
        if (Input.GetMouseButtonDown(1))
        {
            var ogEquipment = m_player.GetComponent<PlayerEquipment>().RightHand.Value;
            m_player.GetComponent<PlayerEquipment>().SetEquipment(PlayerEquipment.Slot.RightHand, wearableNameEnum);
            GetComponent<WeaponSwap>().Init(ogEquipment);
        }
    }

    public override void OnFinishInteraction()
    {
        WeaponSwapCanvas.Instance.Container.SetActive(false);
    }


    public void Init(Wearable.NameEnum wearableNameEnum)
    {
        WearableNameEnum = wearableNameEnum;
        var sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, PlayerGotchi.Facing.Right);
        SpriteRenderer.sprite = sprite;
    }
}
