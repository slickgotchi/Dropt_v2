#region

using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class BlackboardViewModel : ItemContainerViewModel<BlackboardModel, Variable, VariableViewModel>
    {
        public BlackboardViewModel(IDataAsset asset, BlackboardModel model) : base(asset, model)
        {
        }
    }
}