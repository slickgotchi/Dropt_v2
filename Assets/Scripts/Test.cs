using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = WeaponSpriteManager.Instance.GetSprite(Wearable.NameEnum.Handsaw, PlayerGotchi.Facing.Left);
    }
}
