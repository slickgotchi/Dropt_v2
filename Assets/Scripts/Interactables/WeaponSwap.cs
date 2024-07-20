using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponSwap : NetworkBehaviour
{
    public Wearable.NameEnum WearableNameEnum;
    public SpriteRenderer SpriteRenderer;

    private void Awake()
    {
        Init(WearableNameEnum);
    }

    public void Init(Wearable.NameEnum wearableNameEnum)
    {
        WearableNameEnum = wearableNameEnum;
        var sprite = WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, PlayerGotchi.Facing.Right);
        SpriteRenderer.sprite = sprite;
    }
}
