﻿#region

#endregion

#region

using System;

#endregion

namespace CarlosLab.Common
{
    public interface IContainerItem
    {
        string Name { get; internal set; }
        bool IsInContainer { get;}

        internal void HandleItemAdded(IItemContainer container, string name);
        internal void HandleItemRemoved();
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