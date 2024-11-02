using System;
using UnityEngine;
using Unity.Mathematics;
using GotchiHub;

public enum TraitType
{
    NRG, AGG, SPK, BRN, EYS, EYC
};

public static class DroptStatCalculator
{
    private static readonly float TraitDeltaCap = 60;
    private static readonly float expA = 1;
    private static readonly float expR = 2;

    private static readonly LowHigh HpMax = new LowHigh { Low = 500, High = 1000 };
    private static readonly LowHigh Attack = new LowHigh { Low = 50, High = 100 };
    private static readonly LowHigh CriticalChance = new LowHigh { Low = 0.05f, High = 0.25f };
    private static readonly LowHigh ApMax = new LowHigh { Low = 200, High = 500 };
    private static readonly LowHigh DoubleStrike = new LowHigh { Low = 0.01f, High = 0.15f };
    private static readonly LowHigh CriticalDamage = new LowHigh { Low = 1.2f, High = 1.75f };

    // enum for stat picking
    public enum StatType
    {
        Hp, Ap, Armour, Special, Regen, Critical, Melee, Ranged,
    };

    public static float GetDroptStatForGotchiAndAllWearablesByGotchiId(int gotchiId, TraitType traitType)
    {
        var statForGotchi = GetDroptStatForGotchiByGotchiId(gotchiId, traitType);
        var statForWearables = GetDroptStatForAllWearablesByGotchiId(gotchiId, traitType);

        //Debug.Log(traitType.ToString() + " " + statForGotchi + " " + statForWearables);

        return statForGotchi + statForWearables;
    }

    public static float GetDroptStatForGotchiByGotchiId(int gotchiId, TraitType traitType)
    {
        // try onchain first
        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        if (gotchiData != null)
        {
            switch (traitType)
            {
                case TraitType.NRG:
                    return GetDroptStatForGotchiByTraitPoints(gotchiData.numericTraits[0], traitType);
                case TraitType.AGG:
                    return GetDroptStatForGotchiByTraitPoints(gotchiData.numericTraits[1], traitType);
                case TraitType.SPK:
                    return GetDroptStatForGotchiByTraitPoints(gotchiData.numericTraits[2], traitType);
                case TraitType.BRN:
                    return GetDroptStatForGotchiByTraitPoints(gotchiData.numericTraits[3], traitType);
                case TraitType.EYS:
                    return GetDroptStatForGotchiByTraitPoints(gotchiData.numericTraits[4], traitType);
                case TraitType.EYC:
                    return GetDroptStatForGotchiByTraitPoints(gotchiData.numericTraits[5], traitType);
                default: break;
            }
        }

        // try offchain
        var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(gotchiId);
        if (offchainGotchiData != null)
        {
            switch (traitType)
            {
                case TraitType.NRG:
                    return GetDroptStatForGotchiByTraitPoints(offchainGotchiData.numericTraits[0], traitType);
                case TraitType.AGG:
                    return GetDroptStatForGotchiByTraitPoints(offchainGotchiData.numericTraits[1], traitType);
                case TraitType.SPK:
                    return GetDroptStatForGotchiByTraitPoints(offchainGotchiData.numericTraits[2], traitType);
                case TraitType.BRN:
                    return GetDroptStatForGotchiByTraitPoints(offchainGotchiData.numericTraits[3], traitType);
                case TraitType.EYS:
                    return GetDroptStatForGotchiByTraitPoints(offchainGotchiData.numericTraits[4], traitType);
                case TraitType.EYC:
                    return GetDroptStatForGotchiByTraitPoints(offchainGotchiData.numericTraits[5], traitType);
                default: break;
            }
        }

        return 0;
    }

    public static float GetDroptStatForGotchiByTraitPoints(float traitValue, TraitType traitType)
    {
        float traitDelta = Math.Min(Math.Abs(49.5f - traitValue), TraitDeltaCap);
        float x = traitDelta / TraitDeltaCap;

        LowHigh statRange = new LowHigh { Low = 0, High = 1 };
        switch (traitType)
        {
            case TraitType.NRG:
                statRange = HpMax;
                break;
            case TraitType.AGG:
                statRange = Attack;
                break;
            case TraitType.SPK:
                statRange = CriticalChance;
                break;
            case TraitType.BRN:
                statRange = ApMax;
                break;
            case TraitType.EYS:
                statRange = DoubleStrike;
                break;
            case TraitType.EYC:
                statRange = CriticalDamage;
                break;
        }

        float yPlus = (float)Math.Pow(expA * (1 + expR), x);
        float yPlusNormalized = (yPlus - 1) / expR;
        return yPlusNormalized * (statRange.High - statRange.Low) + statRange.Low;
    }

    public static int GetBRS(int nrg, int agg, int spk, int brn, int eys, int eyc)
    {
        int[] traits = new int[] { nrg, agg, spk, brn, eys, eyc };

        int brs = 0;
        for (int i = 0; i < traits.Length; i++)
        {
            if (traits[i] < 50) brs += 100 - traits[i];
            else brs += traits[i] + 1;
        }

        return brs;
    }

    public static int GetBRS(short[] traits)
    {
        int brs = 0;
        for (int i = 0; i < traits.Length; i++)
        {
            if (traits[i] < 50) brs += 100 - traits[i];
            else brs += traits[i] + 1;
        }

        return brs;
    }

    public static float GetDroptStatForAllWearablesByGotchiId(int gotchiId, TraitType traitType)
    {
        // try onchain first
        var gotchiData = GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        if (gotchiData != null)
        {
            float sum = 0;
            foreach (var equippedWearable in gotchiData.equippedWearables)
            {
                sum += GetDroptStatByWearableId(equippedWearable, traitType);
            }
            return sum;
        }

        // try offchain
        var offchainGotchiData = GotchiDataManager.Instance.GetOffchainGotchiDataById(gotchiId);
        if (offchainGotchiData != null)
        {
            float sum = 0;
            foreach (var equippedWearable in offchainGotchiData.equippedWearables)
            {
                sum += GetDroptStatByWearableId(equippedWearable, traitType);
            }
            return sum;
        }

        return 0;
    }


    public static float GetDroptStatByWearableId(int wearableId, TraitType traitType)
    {
        var wearable = WearableManager.Instance.GetWearable(wearableId);
        if (wearable == null)
        {
            return 0;
        }

        switch (traitType)
        {
            case TraitType.NRG:
                return GetDroptStatForWearableByTraitPoints(wearable.Nrg, wearable.Rarity, TraitType.NRG);
            case TraitType.AGG:
                return GetDroptStatForWearableByTraitPoints(wearable.Agg, wearable.Rarity, TraitType.AGG);
            case TraitType.SPK:
                return GetDroptStatForWearableByTraitPoints(wearable.Spk, wearable.Rarity, TraitType.SPK);
            case TraitType.BRN:
                return GetDroptStatForWearableByTraitPoints(wearable.Brn, wearable.Rarity, TraitType.BRN);
            default: break;
        }

        return 0;
    }

    public static float GetDroptStatForWearableByTraitPoints(int traitPoints, Wearable.RarityEnum rarityEnum, TraitType traitType)
    {
        traitPoints = math.abs(traitPoints);

        switch (rarityEnum)
        {
            case Wearable.RarityEnum.Common:
                switch (traitType)
                {
                    case TraitType.NRG: return 25 * traitPoints;
                    case TraitType.AGG: return 2.5f * traitPoints;
                    case TraitType.SPK: return 0.02f * traitPoints;
                    case TraitType.BRN: return 12.5f * traitPoints;
                    default: break;
                }
                break;
            case Wearable.RarityEnum.Uncommon:
                switch (traitType)
                {
                    case TraitType.NRG: return 37.5f * traitPoints;
                    case TraitType.AGG: return 3.75f * traitPoints;
                    case TraitType.SPK: return 0.015f * traitPoints;
                    case TraitType.BRN: return 18.75f * traitPoints;
                    default: break;
                }
                break;
            case Wearable.RarityEnum.Rare:
                switch (traitType)
                {
                    case TraitType.NRG: return 50 * traitPoints;
                    case TraitType.AGG: return 5f * traitPoints;
                    case TraitType.SPK: return 0.017f * traitPoints;
                    case TraitType.BRN: return 25f * traitPoints;
                    default: break;
                }
                break;
            case Wearable.RarityEnum.Legendary:
                switch (traitType)
                {
                    case TraitType.NRG: return 56 * traitPoints;
                    case TraitType.AGG: return 5.5f * traitPoints;
                    case TraitType.SPK: return 0.018f * traitPoints;
                    case TraitType.BRN: return 28f * traitPoints;
                    default: break;
                }
                break;
            case Wearable.RarityEnum.Mythical:
                switch (traitType)
                {
                    case TraitType.NRG: return 60 * traitPoints;
                    case TraitType.AGG: return 6f * traitPoints;
                    case TraitType.SPK: return 0.02f * traitPoints;
                    case TraitType.BRN: return 30f * traitPoints;
                    default: break;
                }
                break;
            case Wearable.RarityEnum.Godlike:
                switch (traitType)
                {
                    case TraitType.NRG: return 70 * traitPoints;
                    case TraitType.AGG: return 6.67f * traitPoints;
                    case TraitType.SPK: return 0.023f * traitPoints;
                    case TraitType.BRN: return 35f * traitPoints;
                    default: break;
                }
                break;
            default: break;
        }


        return 0;
    }

}

[System.Serializable]
public struct LowHigh
{
    public float Low;
    public float High;
}
