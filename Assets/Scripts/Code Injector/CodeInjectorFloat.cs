using UnityEngine;
using System;

[Serializable]
public class CodeInjectorFloat : CodeInjectorVariable<float>
{
    public override bool IsChanged()
    {
        return Value != UpdatedValue;
    }

    public override void SetValue(float value)
    {
        Value = value;
        UpdatedValue = value;
        PlayerPrefs.SetFloat(VariableType.ToString(), value);
        CurrentIndex = Inputs.FindIndex(item => item.Value.ToString("F2").Equals(Value.ToString("F2")));
    }

    public override void Initialize()
    {
        Value = PlayerPrefs.GetFloat(VariableType.ToString(), DefalutValue);
        UpdatedValue = Value;
        CurrentIndex = Inputs.FindIndex(item => item.Value.ToString("F2").Equals(Value.ToString("F2")));
    }

    public override void Reset()
    {
        UpdatedValue = Value;
        CurrentIndex = Inputs.FindIndex(item => item.Value.ToString("F2").Equals(Value.ToString("F2")));
    }

    public override float GetMultiplier()
    {
        return Inputs.Find(item => item.Value.ToString("F2").Equals(Value.ToString("F2"))).Multiplier;
    }

    public override string ToString()
    {
        return $"{(int)(UpdatedValue * 100)}%";
    }
}