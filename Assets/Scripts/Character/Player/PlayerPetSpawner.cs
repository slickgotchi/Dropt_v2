using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerPetSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _spawnPetCanvas;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _spawnPetCanvas.SetActive(IsOwner);
    }

    public void SpawnPet()
    {
        if (IsServer)
        {
            Debug.Log("NETWORK OBJECT ID -> " + NetworkObjectId);
            PetsManager.Instance.SpawnPet(PetType.FoxyTail, transform.position, NetworkObjectId);
        }
        else
        {
            SpawnPetServerRpc(PetType.FoxyTail, transform.position, NetworkObjectId);
        }
    }

    [ServerRpc]
    public void SpawnPetServerRpc(PetType petType, Vector3 position, ulong senderId)
    {
        PetsManager.Instance.SpawnPet(petType, position, senderId);
    }

    private PetType GetRandomPetType()
    {
        Array values = Enum.GetValues(typeof(PetType));
        System.Random random = new();
        return (PetType)values.GetValue(random.Next(values.Length));
    }
}
