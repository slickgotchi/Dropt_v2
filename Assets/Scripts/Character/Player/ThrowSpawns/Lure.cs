using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Lure : NetworkBehaviour
{
    public int Hp = 100;
    public NetworkVariable<Wearable.NameEnum> WearableNameEnum = new NetworkVariable<Wearable.NameEnum>();

    public SpriteRenderer BodySpriteRenderer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            GetComponent<NetworkCharacter>().currentStaticStats.HpMax = Hp;
            GetComponent<NetworkCharacter>().currentDynamicStats.HpCurrent = Hp;
        }

        if (IsClient)
        {
            transform.Find("Body").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(WearableNameEnum.Value, PlayerGotchi.Facing.Front);
        }
    }
}
