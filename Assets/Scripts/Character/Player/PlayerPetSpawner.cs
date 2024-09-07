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
            PetsManager.Instance.SpawnPet(GetRandomPetType(), transform.position, OwnerClientId);
        }
        else
        {
            SpawnPetServerRpc(GetRandomPetType(), transform.position, OwnerClientId);
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
