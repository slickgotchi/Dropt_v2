using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStat
{
    NA,

    // primary (from NetworkCharacter)
    HpMax,
    HpBuffer,
    AttackPower,
    CriticalChance,
    ApMax,
    ApBuffer,
    DoubleStrikeChance,
    CriticalDamage,
    MoveSpeed,
    Accuracy,
    Evasion,
    DamageReduction,

    // attack
    DebuffEffectiveness,
    SpecialCooldownReduction,
    Piercing,
    SpecialCostReduction,
    AttackRange,

    // defense
    ReduceMeleeDamage,
    ReduceRangedDamage,
    ReduceElementalDamage,
    Block,
    ApLeech,
    HpLeech,
    EssenceLeech,

    // environment
    Magnetism,
    ExtraDash,
    Purveying,

    // debuffs
    Poison,
    Weak,
    RootChance,
    BleedChance,
    BurnChance,
    SlowChance,
    ShockChance,
    BlindChance,

    // regen
    ApRegen,
}