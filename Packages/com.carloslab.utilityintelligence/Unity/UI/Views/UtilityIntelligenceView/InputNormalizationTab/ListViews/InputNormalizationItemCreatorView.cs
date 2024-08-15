using System;
using System.Reflection;
using CarlosLab.Common.Extensions;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationItemCreatorView : NameTypeItemCreatorView<InputNormalizationListViewModel, InputNormalizationItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(InputNormalization);
        
        protected override string FormatListItem(Type inputNormalizationType)
        {
            if (inputNormalizationType == null)
                return "None";

            Type inputValueType = GetInputValueType(inputNormalizationType, typeof(InputNormalization<>));
            if (inputValueType == null)
                return "None";
            
            var categoryAttribute = inputNormalizationType.GetCustomAttribute<CategoryAttribute>();
            if (categoryAttribute != null)
                return $"{categoryAttribute.Category}/{inputNormalizationType.Name}";

            string inputValueTypeName = inputValueType.GetName();

            return $"{inputValueTypeName}/{inputNormalizationType.Name}";
        }

        protected override int CompareChoices(Type choice1, Type choice2)
        {
            var categoryAttribute1 = choice1.GetCustomAttribute<CategoryAttribute>();
            var categoryAttribute2 = choice2.GetCustomAttribute<CategoryAttribute>();
            int result;
            if (categoryAttribute1 == null && categoryAttribute2 == null)
            {
                Type inputValueType1 = GetInputValueType(choice1, typeof(InputNormalization<>));
                if (inputValueType1 == null)
                    return -1;

                Type inputValueType2 = GetInputValueType(choice2, typeof(InputNormalization<>));
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

        private Type GetInputValueType(Type inputNormalzationType, Type inputNormalizationTypeDefinition)
        {
            Type inputValueType = null;
            while (inputNormalzationType.BaseType != null)
            {
                inputNormalzationType = inputNormalzationType.BaseType;
                if (inputNormalzationType.IsGenericType && inputNormalzationType.GetGenericTypeDefinition() == inputNormalizationTypeDefinition)
                {
                    inputValueType = inputNormalzationType.GetGenericArguments()[0];
                    break;
                }
            }

            return inputValueType;
        }
    }
}