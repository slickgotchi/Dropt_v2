using System;
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

    public async void SpawnPet(PetType petType, Vector3 position, ulong ownerObjectId)
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
        pet.GetComponent<PetController>().InitlaizePet(ownerObjectId, GetDamageMultiplier(petType));
        NetworkObject networkObj = pet.GetComponent<NetworkObject>();
        networkObj.Spawn();
    }

    private float GetDamageMultiplier(PetType petType)
    {
        Wearable.NameEnum nameEnum = Enum.Parse<Wearable.NameEnum>(petType.ToString(), true);
        Wearable.RarityEnum rarityEnum = WearableManager.Instance.GetWearable(nameEnum).Rarity;

        switch (rarityEnum)
        {
            case Wearable.RarityEnum.Common:
                return 1.0f;
            case Wearable.RarityEnum.Uncommon:
                return 1.1f;
            case Wearable.RarityEnum.Rare:
                return 1.25f;
            case Wearable.RarityEnum.Legendary:
                return 1.6f;
            case Wearable.RarityEnum.Mythical:
                return 2.0f;
            case Wearable.RarityEnum.Godlike:
                return 2.5f;
        }

        return 0;
    }
}