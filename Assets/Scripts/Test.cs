using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = WeaponSpriteManager.Instance.GetSprite(Wearable.NameEnum.SlicksSurfboard, PlayerGotchi.Facing.Left);

        var wearable = WearableManager.Instance.GetWearable(Wearable.NameEnum.SlicksSurfboard);
        Debug.Log(wearable.Name + " " + wearable.NameType + " " + wearable.WeaponType);
    }
}
