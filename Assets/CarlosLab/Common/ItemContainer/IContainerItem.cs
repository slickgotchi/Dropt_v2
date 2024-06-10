#region

#endregion

#region

using System;

#endregion

namespace CarlosLab.Common
{
    public interface IContainerItem
    {
        string Name { get; internal set; }
        bool IsInContainer { get; internal set; }

        internal void OnItemAdded(string name)
        {
            Name = name;
            IsInContainer = true;
        }

        internal void OnItemRemoved()
        {
            IsInContainer = false;
        }
    }

    public interface IContainerItemValue : IContainerItem
    {
        object ValueObject { get; }
        Type ValueType { get; }
    }

    public interface IContainerItemValue<TValueType> : IContainerItemValue
    {
        TValueType Value { get; }
    }
}