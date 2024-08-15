#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public interface ITargetViewModel : IViewModel
    {
        string TargetName { get; }
    }
}