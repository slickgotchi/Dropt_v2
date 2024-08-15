#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ValueItemView<TViewModel> : ValueItemView<TViewModel, UtilityIntelligenceView>
        where TViewModel : class, IItemViewModel, IValueViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {
    }
}