using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Buff
{
    public enum Modifier
    {
        Add, Subtract,
        Multiply, Divide,
        Set,
    }

    public CharacterStat BuffStat;
    public Modifier BuffModifier;
    public float Value;
}

