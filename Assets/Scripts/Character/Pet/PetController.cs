using UnityEngine;
using Unity.Netcode;

public class PetController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log("CLIENT ID -> " + OwnerClientId);
    }
}
