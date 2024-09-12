using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class CodeInjectorVariable<T>
{
    [SerializeField] protected CodeInjector.Variable VariableType;
    public List<CodeInjectorVariableInput<T>> Inputs;
    public T DefalutValue;
    protected T Value;
    protected T UpdatedValue;
    protected int CurrentIndex;

    public abstract void Initialize();
    public abstract bool IsChanged();
    public abstract void SetValue(T value);
    public abstract void Reset();
    public abstract float GetMultiplier();

    public void Add()
    {
        CurrentIndex++;
        if (CurrentIndex > Inputs.Count - 1)
        {
            CurrentIndex = Inputs.Count - 1;
        }
        UpdatedValue = Inputs[CurrentIndex].Value;
    }

    public void Subtract()
    {
        CurrentIndex--;
        if (CurrentIndex < 0)
        {
            CurrentIndex = 0;
        }
        UpdatedValue = Inputs[CurrentIndex].Value;
    }

    public T GetUpdatedValue()
    {
        return UpdatedValue;
    }
}

[System.Serializable]
public class CodeInjectorVariableInput<T>
{
    public float Multiplier;
    public T Value;
}

