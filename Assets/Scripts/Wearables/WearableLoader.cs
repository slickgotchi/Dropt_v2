using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO;

public class WearableLoader : MonoBehaviour
{
    public TextAsset wearableCsv;

    private static readonly CultureInfo m_culture = CultureInfo.CreateSpecificCulture("en-US");

    void Start()
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
                SecondaryBuff = (CharacterStat)System.Enum.Parse(typeof(CharacterStat), row[12]),
                SecondaryBuffValue = ParseFloat(row[13]),
                TertiaryBuff = (CharacterStat)System.Enum.Parse(typeof(CharacterStat), row[14]),
                TertiaryBuffValue = ParseFloat(row[15]),
                BaseDescription = row[16],
//<<<<<<< HEAD
                AttackName = row[17],
                AttackDescription = row[18],
                HoldName = row[19],
                HoldDescription = row[20],
                SpecialName = row[21],
                SpecialDescription = row[22],
                EffectDuration = int.Parse(row[23]),
                AttackView = (PlayerGotchi.Facing)System.Enum.Parse(typeof(PlayerGotchi.Facing), row[24]),
                AttackAngle = float.Parse(row[25]),
//=======
//                AttackDescription = row[17],
//                HoldDescription = row[18],
//                SpecialDescription = row[19],
//                EffectDuration = int.Parse(row[20]),
//                AttackView = (PlayerGotchi.Facing)System.Enum.Parse(typeof(PlayerGotchi.Facing), row[21]),
//                AttackAngle = ParseFloat(row[22]),
//>>>>>>> 65f8b13b ([CHG] finalize chests logic)
            };

            WearableManager.Instance.AddWearable(wearable);
        }

        Debug.Log(WearableManager.Instance.GetWearableCount() + " wearables loaded.");
    }

    private static float ParseFloat(string value)
    {
        return Convert.ToSingle(value, m_culture);
    }
}
