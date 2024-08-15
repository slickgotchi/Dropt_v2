#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionItemViewModel : BaseItemViewModel<ActionModel, ActionListViewModel>, ITypeNameViewModel
    {
        public string TypeName => Model?.RuntimeType.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;
    }
}