using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LilEssence : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            PlayerDungeonData playerData = collision.gameObject.GetComponent<PlayerDungeonData>();
            if (playerData != null)
            {
                Debug.Log("Add 10 essence to player");
                playerData.AddEssence(10);
                if (IsServer) gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
