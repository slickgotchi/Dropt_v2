#region

using System;
using System.Reflection;
using CarlosLab.Common.Extensions;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputItemCreatorView : NameTypeItemCreatorView<InputListViewModel, InputItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(Input);

        protected override string FormatListItem(Type inputType)
        {
            if (inputType == null)
                return "None";

            Type inputValueType = GetInputValueType(inputType, typeof(Input<>));
            if (inputValueType == null)
                return "None";
            
            var categoryAttribute = inputType.GetCustomAttribute<CategoryAttribute>();
            if (categoryAttribute != null)
                return $"{categoryAttribute.Category}/{inputType.Name}";

            string inputValueTypeName = inputValueType.GetName();

            return $"{inputValueTypeName}/{inputType.Name}";
        }
        
        protected override int CompareChoices(Type choice1, Type choice2)
        {
            var categoryAttribute1 = choice1.GetCustomAttribute<CategoryAttribute>();
            var categoryAttribute2 = choice2.GetCustomAttribute<CategoryAttribute>();
            int result;
            if (categoryAttribute1 == null && categoryAttribute2 == null)
            {
                Type inputValueType1 = GetInputValueType(choice1, typeof(Input<>));
                if (inputValueType1 == null)
                    return -1;

                Type inputValueType2 = GetInputValueType(choice2, typeof(Input<>));
                if (inputValueType2 == null)
                    return 1;

                string inputValueTypeName1 = inputValueType1.GetName();
                string inputValueTypeName2 = inputValueType2.GetName();

                result = string.CompareOrdinal(inputValueTypeName1, inputValueTypeName2);
                if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
                
                return result;
            }

            if (categoryAttribute1 == null)
                return 1;

            if (categoryAttribute2 == null)
                return -1;

            result = string.CompareOrdinal(categoryAttribute1.Category, categoryAttribute2.Category);
            if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
            return result;
        }
        
        private Type GetInputValueType(Type inputType, Type inputTypeDefinition)
        {
            Type inputValueType = null;
            while (inputType.BaseType != null)
            {
                inputType = inputType.BaseType;
                if (inputType.IsGenericType && inputType.GetGenericTypeDefinition() == inputTypeDefinition)
                {
                    inputValueType = inputType.GetGenericArguments()[0];
                    break;
                }
            }

            return inputValueType;
        }
    }
}