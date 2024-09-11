using UnityEngine;

[System.Serializable]
public class CodeInjectorFloat : CodeInjectorVariable<float>
{
    public enum Type
    {
        Normal,
        Percentage
    }

    public Type ValueType;

    public override void Add()
    {
        UpdatedValue += DeltaValue;
        if (UpdatedValue > MaxValue)
        {
            UpdatedValue = MaxValue;
        }
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
        return ValueType == Type.Percentage
               ? $"{(int)(UpdatedValue * 100)}%"
               : $"{UpdatedValue:F1}";
    }

    public override bool IsChanged()
    {
        return Value != UpdatedValue;
    }

    public override void SetValue(float value)
    {
        Value = value;
        UpdatedValue = value;
        PlayerPrefs.SetFloat(VariableType.ToString(), value);
    }

    public override void Initialize()
    {
        Value = PlayerPrefs.GetFloat(VariableType.ToString(), DefalutValue);
        UpdatedValue = Value;
    }
}
