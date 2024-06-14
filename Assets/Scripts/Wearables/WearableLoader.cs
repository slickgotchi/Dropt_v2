using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WearableLoader : MonoBehaviour
{
    public TextAsset wearableCsv;

    void Awake()
    {
        LoadWearables();
    }

    void LoadWearables()
    {
        if (wearableCsv == null)
        {
            Debug.LogError("CSV file not assigned in the inspector");
            return;
        }

        string[] data = wearableCsv.text.Split(new char[] { '\n' });

        // Skip header line
        for (int i = 1; i < data.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(data[i])) continue;

            string[] row = data[i].Split(',');

            Wearable wearable = new Wearable
            {
                Slot = (Wearable.SlotEnum)System.Enum.Parse(typeof(Wearable.SlotEnum), row[0]),
                Id = int.Parse(row[1]),
                Name = row[2],
                NameType = (Wearable.NameEnum)System.Enum.Parse(typeof(Wearable.NameEnum), row[3]),
                Rarity = (Wearable.RarityEnum)System.Enum.Parse(typeof(Wearable.RarityEnum), row[4]),
                Nrg = int.Parse(row[5]),
                Agg = int.Parse(row[6]),
                Spk = int.Parse(row[7]),
                Brn = int.Parse(row[8]),
                WeaponType = (Wearable.WeaponTypeEnum)System.Enum.Parse(typeof(Wearable.WeaponTypeEnum), row[9]),
                SpecialAp = int.Parse(row[10]),
                SpecialCooldown = int.Parse(row[11]),
                SecondaryBuff = (Wearable.BuffEnum)System.Enum.Parse(typeof(Wearable.BuffEnum), row[12]),
                SecondaryBuffValue = float.Parse(row[13]),
                TertiaryBuff = (Wearable.BuffEnum)System.Enum.Parse(typeof(Wearable.BuffEnum), row[14]),
                TertiaryBuffValue = float.Parse(row[15]),
                SwapDescription = row[16]
            };

            WearableManager.Instance.AddWearable(wearable);
        }

        Debug.Log(WearableManager.Instance.GetWearableCount() + " wearables loaded.");
    }
}
