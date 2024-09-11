using UnityEngine;

[System.Serializable]
public abstract class CodeInjectorVariable<T>
{
    [SerializeField] protected CodeInjector.Variable VariableType;
    public T MinValue;
    public T MaxValue;
    public T DefalutValue;
    public T Value;
    public T UpdatedValue;
    public T DeltaValue;

    public abstract void Initialize();
    public abstract void Add();
    public abstract void Subtract();
    public abstract bool IsChanged();
    public abstract void SetValue(T value);

    public void Reset()
    {
        UpdatedValue = Value;
    }
}
