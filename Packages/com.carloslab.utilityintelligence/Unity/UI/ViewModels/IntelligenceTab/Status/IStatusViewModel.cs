#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public interface IStatusViewModel : IViewModel
    {
        event Action<Status> StatusChanged;
        Status CurrentStatus { get; }
    }
}