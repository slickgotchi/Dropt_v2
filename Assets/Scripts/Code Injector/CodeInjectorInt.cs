using UnityEngine;

[System.Serializable]
public class CodeInjectorInt : CodeInjectorVariable<int>
{
    public override void Initialize()
    {
        Value = PlayerPrefs.GetInt(VariableType.ToString(), DefalutValue);
        UpdatedValue = Value;
        CurrentIndex = Inputs.FindIndex(item => item.Value.Equals(Value));
    }

    public override bool IsChanged()
    {
        return Value != UpdatedValue;
    }

    public override void ResetUpdatedValue()
    {
        UpdatedValue = Value;
        CurrentIndex = Inputs.FindIndex(item => item.Value.Equals(UpdatedValue));
    }

    public override void SetValue(int value)
    {
        Value = value;
        UpdatedValue = value;
        PlayerPrefs.SetInt(VariableType.ToString(), value);
        CurrentIndex = Inputs.FindIndex(item => item.Value.Equals(Value));
    }

    public override float GetMultiplier()
    {
        return Inputs.Find(item => item.Value.Equals(UpdatedValue)).Multiplier;
    }

    public override string ToString()
    {
        return $"{UpdatedValue}";
    }

    public override void ResetToDefault()
    {
        UpdatedValue = DefalutValue;
        CurrentIndex = Inputs.FindIndex(item => item.Value.Equals(UpdatedValue));
    }
}
