using UnityEngine;

[System.Serializable]
public class CodeInjectorInt : CodeInjectorVariable<int>
{
    public override void Add()
    {
        UpdatedValue += DeltaValue;
        if (UpdatedValue > MaxValue)
        {
            UpdatedValue = MaxValue;
        }
    }

    public override void Initialize()
    {
        Value = PlayerPrefs.GetInt(VariableType.ToString(), DefalutValue);
        UpdatedValue = Value;
    }

    public override bool IsChanged()
    {
        return Value != UpdatedValue;
    }

    public override void SetValue(int value)
    {
        Value = value;
        UpdatedValue = value;
        PlayerPrefs.SetInt(VariableType.ToString(), value);
    }

    public override void Subtract()
    {
        UpdatedValue -= DeltaValue;
        if (UpdatedValue < MinValue)
        {
            UpdatedValue = MinValue;
        }
    }

    public override string ToString()
    {
        return $"{UpdatedValue}";
    }
}
