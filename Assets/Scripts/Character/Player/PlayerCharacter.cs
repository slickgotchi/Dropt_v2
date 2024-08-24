using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PlayerCharacter : NetworkCharacter
{
    [Header("Equipment Buff Objects")]
    public BuffObject bodyBuffObject;
    public BuffObject faceBuffObject;
    public BuffObject eyesBuffObject;
    public BuffObject headBuffObject;
    public BuffObject rightHandBuffObject;
    public BuffObject leftHandBuffObject;
    public BuffObject petBuffObject;

    public void InitGotchiStats(int gotchiId)
    {
        if (!IsServer) return;

        // get gotchi data
        var gotchiData = GotchiHub.GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        if (gotchiData == null)
        {
            Debug.LogWarning("No gotchiData avaiable for gotchi ID: " + gotchiId);
            return;
        }

        // update character stats
        var hp = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[0], TraitType.NRG);
        var attack = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[1], TraitType.AGG);
        var critChance = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[2], TraitType.SPK);
        var ap = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[3], TraitType.BRN);
        var doubleStrikeChance = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[4], TraitType.EYS);
        var critDamage = DroptStatCalculator.GetPrimaryGameStat(gotchiData.numericTraits[5], TraitType.EYC);

        baseHpMax = hp;
        baseHpCurrent = hp;
        baseHpBuffer = 0;

        baseAttackPower = attack;

        baseCriticalChance = critChance;

        baseApMax = ap;
        baseApCurrent = ap;
        baseApBuffer = 0;

        baseDoubleStrikeChance = doubleStrikeChance;

        baseCriticalDamage = critDamage;

        Debug.Log("Setting player stats, hp: " + hp + ", atk: " + attack + ", crit: " + critChance + ", ap: " + ap + ", doubleStrike: " + doubleStrikeChance + ", critDmg: " + critDamage);

        RecalculateStats();
    }

    public void InitWearableBuffs(int gotchiId)
    {
        if (!IsServer) return;

        // get gotchi data
        var gotchiData = GotchiHub.GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        if (gotchiData == null)
        {
            Debug.LogWarning("No gotchiData avaiable for gotchi ID: " + gotchiId);
            return;
        }

        // remove all existing buffs
        RemoveBuffObject(bodyBuffObject);
        RemoveBuffObject(faceBuffObject);
        RemoveBuffObject(eyesBuffObject);
        RemoveBuffObject(headBuffObject);
        RemoveBuffObject(rightHandBuffObject);
        RemoveBuffObject(leftHandBuffObject);
        RemoveBuffObject(petBuffObject);

        // get ref to equipment
        var equipment = gotchiData.equippedWearables;

        // create new buffs based on gotchi data
        bodyBuffObject = CreateBuffObjectFromWearableId(equipment[0]);
        faceBuffObject = CreateBuffObjectFromWearableId(equipment[1]);
        eyesBuffObject = CreateBuffObjectFromWearableId(equipment[2]);
        headBuffObject = CreateBuffObjectFromWearableId(equipment[3]);
        rightHandBuffObject = CreateBuffObjectFromWearableId(equipment[4]);
        leftHandBuffObject = CreateBuffObjectFromWearableId(equipment[5]);
        petBuffObject = CreateBuffObjectFromWearableId(equipment[6]);

        // add the buffs
        AddBuffObject(bodyBuffObject);
        AddBuffObject(faceBuffObject);
        AddBuffObject(eyesBuffObject);
        AddBuffObject(headBuffObject);
        AddBuffObject(rightHandBuffObject);
        AddBuffObject(leftHandBuffObject);
        AddBuffObject(petBuffObject);
    }

    BuffObject CreateBuffObjectFromWearableId(int wearableId)
    {
        BuffObject buffObject = ScriptableObject.CreateInstance<BuffObject>();
        var wearable = WearableManager.Instance.GetWearable(wearableId);
        if (wearable == null)
        {
            return buffObject;
        }


        // Ensure the buffs list is initialized
        if (buffObject.buffs == null)
        {
            buffObject.buffs = new List<Buff>();
        }

        var hp = GetWearableStat(wearable.Nrg, wearable.Rarity, TraitType.NRG);
        var atk = GetWearableStat(wearable.Agg, wearable.Rarity, TraitType.AGG);
        var crit = GetWearableStat(wearable.Spk, wearable.Rarity, TraitType.SPK);
        var ap = GetWearableStat(wearable.Brn, wearable.Rarity, TraitType.BRN);

        Debug.Log("Create buff from wearable: " + wearable.Name + ", hp + " + hp + ", atk + " + atk + ", crit + " + crit + ", ap + " + ap);

        // hp buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.HpMax,
            Value = hp
        });


        // attack buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.AttackPower,
            Value = atk
        });

        // crit chance buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.CriticalChance,
            Value = crit
        });

        // ap buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.ApMax,
            Value = ap
        });

        return buffObject;
    }

    float GetWearableStat(int traitPoints, Wearable.RarityEnum rarityEnum, TraitType traitType)
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
