#region

using System;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        ActionItemCreatorView : TypeItemCreatorView<ActionListViewModel, ActionViewModel>
    {
        protected override Type BaseType { get; } = typeof(ActionTask);
    }
}