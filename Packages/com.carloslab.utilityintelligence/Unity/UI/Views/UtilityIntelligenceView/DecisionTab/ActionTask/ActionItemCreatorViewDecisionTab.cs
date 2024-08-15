#region

using System;
using System.Reflection;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        ActionItemCreatorViewDecisionTab : BasicTypeItemCreatorView<ActionListViewModel, ActionItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(ActionTask);

        protected override string FormatListItem(Type type)
        {
            if (type == null)
                return "None";

            var categoryAttribute = type.GetCustomAttribute<CategoryAttribute>();
            if (categoryAttribute == null) return base.FormatListItem(type);
            
            return $"{categoryAttribute.Category}/{type.Name}";
        }

        protected override int CompareChoices(Type choice1, Type choice2)
        {
            var categoryAttribute1 = choice1.GetCustomAttribute<CategoryAttribute>();
            var categoryAttribute2 = choice2.GetCustomAttribute<CategoryAttribute>();

            if (categoryAttribute1 == null && categoryAttribute2 == null)
                return string.CompareOrdinal(choice1.Name, choice2.Name);

            if (categoryAttribute1 == null)
                return 1;

            if (categoryAttribute2 == null)
                return -1;

            int result = string.CompareOrdinal(categoryAttribute1.Category, categoryAttribute2.Category);
            if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
            return result;
        }
    }
}