using System;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterEditorItemCreatorView: NameTypeItemCreatorView<TargetFilterEditorListViewModel, TargetFilterEditorViewModel>
    {
        protected override Type BaseType { get; } = typeof(TargetFilter);
    }
}