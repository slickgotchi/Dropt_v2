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

        protected override string FormatListItem(Type inputNormalizationType)
        {
            if (inputNormalizationType == null)
                return "None";

            return inputNormalizationType.Name.Replace("Variable", string.Empty);
        }

        protected override string FormatSelectedItem(Type inputType)
        {
            if (inputType == null)
                return "None";

            return inputType.Name.Replace("Variable", string.Empty);
        }

        protected override int CompareChoices(Type choice1, Type choice2)
        {
            string typeName1 = choice1.Name.Replace("Variable", string.Empty);
            string typeName2 = choice2.Name.Replace("Variable", string.Empty);

            return string.CompareOrdinal(typeName1, typeName2);
        }
    }
}