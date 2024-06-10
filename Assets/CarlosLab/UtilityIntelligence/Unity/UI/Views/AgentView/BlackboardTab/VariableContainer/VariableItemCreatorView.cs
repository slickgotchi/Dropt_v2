#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableItemCreatorView : NameTypeItemCreatorView<BlackboardViewModel, VariableViewModel>
    {
        protected override Type BaseType { get; } = typeof(Variable);

        protected override string FormatListItem(Type inputType)
        {
            if (inputType == null)
                return "NONE";

            return inputType.Name.Replace("Variable", string.Empty);
        }

        protected override string FormatSelectedItem(Type inputType)
        {
            if (inputType == null)
                return "NONE";

            return inputType.Name.Replace("Variable", string.Empty);
        }
    }
}