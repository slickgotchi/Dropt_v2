using System;
using UnityEngine;

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

    public static float GetPrimaryGameStat(float traitValue, TraitType traitType)
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
}

[System.Serializable]
public struct LowHigh
{
    public float Low;
    public float High;
}
