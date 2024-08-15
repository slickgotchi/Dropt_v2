#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public interface IScoreViewModel : IViewModel
    {
        float Score { get; }
    }
}