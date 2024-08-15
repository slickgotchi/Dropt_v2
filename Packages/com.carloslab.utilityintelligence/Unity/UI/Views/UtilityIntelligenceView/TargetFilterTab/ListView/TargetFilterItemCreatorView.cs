using System;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterItemCreatorView: NameTypeItemCreatorView<TargetFilterListViewModel, TargetFilterItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(TargetFilter);
    }
}