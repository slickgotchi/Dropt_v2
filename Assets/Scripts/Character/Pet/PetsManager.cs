using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PetsManager : NetworkBehaviour
{
    public static PetsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void SpawnPet(PetType petType, Vector3 position, ulong senderId)
    {
        if (!IsServer)
        {
            return;
        }

        ResourceRequest request = Resources.LoadAsync<GameObject>($"Pets/{petType}");

        while (!request.isDone)
        {
            await UniTask.Yield();
        }

        GameObject petPrefab = (GameObject)request.asset;
        var pet = Instantiate(petPrefab, position, Quaternion.identity);
        var networkObj = pet.GetComponent<NetworkObject>();
        networkObj.SpawnWithOwnership(senderId);
    }
}