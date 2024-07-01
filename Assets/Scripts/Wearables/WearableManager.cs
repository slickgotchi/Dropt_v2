using System.Collections.Generic;
using UnityEngine;

public class WearableManager : MonoBehaviour
{
    public static WearableManager Instance { get; private set; }
    private Dictionary<int, Wearable> wearablesById = new Dictionary<int, Wearable>();
    public Dictionary<Wearable.NameEnum, Wearable> wearablesByNameEnum = new Dictionary<Wearable.NameEnum, Wearable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddWearable(Wearable wearable)
    {
        if (!wearablesById.ContainsKey(wearable.Id))
        {
            wearablesById[wearable.Id] = wearable;
        }

        if (!wearablesByNameEnum.ContainsKey(wearable.NameType))
        {
            wearablesByNameEnum[wearable.NameType] = wearable;
        }
    }

    public Wearable GetWearable(int id)
    {
        wearablesById.TryGetValue(id, out Wearable wearable);
        return wearable;
    }

    public Wearable GetWearable(Wearable.NameEnum nameEnum)
    {
        wearablesByNameEnum.TryGetValue(nameEnum, out Wearable wearable);
        return wearable;
    }

    public int GetWearableCount()
    {
        return wearablesById.Count;
    }
}
