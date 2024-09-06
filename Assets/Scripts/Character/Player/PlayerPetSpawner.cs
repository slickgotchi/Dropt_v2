using Unity.Netcode;
using UnityEngine;

public class PlayerPetSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _spawnPetCanvas;
    //[SerializeField] private NetworkObject m_pet;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _spawnPetCanvas.SetActive(IsOwner);
    }

    public void SpawnPet()
    {
        //PetsManager petsManager = FindObjectOfType<PetsManager>();
        //petsManager?.SpawnPet(transform.position);
        SpawnPetServerRpc(transform.position);
    }

    [Rpc(SendTo.Server)]
    public void SpawnPetServerRpc(Vector3 position)
    {
        if (!IsServer)
        {
            return;
        }
        PetsManager.Instance.SpawnPet(position);
    }
}
