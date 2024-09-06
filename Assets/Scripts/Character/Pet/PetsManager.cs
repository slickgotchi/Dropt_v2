using Core.Pool;
using Unity.Netcode;
using UnityEngine;

public class PetsManager : NetworkBehaviour
{
    public static PetsManager Instance { get; private set; }

    [SerializeField] private GameObject m_pet;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnPet(Vector3 position)
    {
        if (!IsServer) return;
        InstantiatePet(position);
        //if (!IsServer && IsOwner)
        //{
        //    Debug.Log("CALL SERVER RPC");
        //    SpawnPetServerRpc(position);
        //}
        //else
        //{
        //    InstantiatePet(position);
        //}
    }

    //[Rpc(SendTo.Server)]
    //public void SpawnPetServerRpc(Vector3 position)
    //{
    //    InstantiatePet(position);
    //}

    public void InstantiatePet(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        var pet = Instantiate(m_pet, position, Quaternion.identity);

        var networkObj = pet.GetComponent<NetworkObject>();
        if (!networkObj.IsSpawned)
        {
            networkObj.Spawn();
        }
    }

    private NetworkObject GetFromPool(GameObject prefab, Vector3 randPosition)
    {
        return NetworkObjectPool.Singleton
            .GetNetworkObject(prefab, randPosition, Quaternion.identity);
    }
}
