#region

using System;
using CarlosLab.Common.Extensions;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputItemCreatorView : NameTypeItemCreatorView<InputListViewModel, InputItemViewModel>
    {
        protected override Type BaseType { get; } = typeof(Input);

#if UNITY_6000_0_OR_NEWER
        protected override string FormatListItem(Type inputType)
        {
            if (inputType == null)
                return "None";

            Type inputValueType = GetInputValueType(inputType, typeof(Input<>));
            if (inputValueType == null)
                return "None";

            string inputValueTypeName = inputValueType.GetName();

            return $"{inputValueTypeName}/{inputType.Name}";
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
        
#endif
    }
}