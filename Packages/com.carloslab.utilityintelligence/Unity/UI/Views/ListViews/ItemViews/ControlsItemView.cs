#region

using CarlosLab.UtilityIntelligence.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public class ControlsItemView<TItemViewModel> : ControlsItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {

    }
}