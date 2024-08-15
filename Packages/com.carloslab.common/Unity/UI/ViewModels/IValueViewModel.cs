#region

using System;

#endregion

namespace CarlosLab.Common.UI
{
    public interface IValueViewModel : IViewModel
    {
        object ValueObject { get; set; }
        Type ValueType { get; }
    }
}