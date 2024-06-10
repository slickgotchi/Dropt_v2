#region

using System.Collections.Generic;
using System.Reflection;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class GenericModel<TRuntime> : CarlosLab.Common.GenericModel<TRuntime>, IGenericModel
        where TRuntime : class
    {
        private readonly List<FieldInfo> variableReferenceFields = new();

        public IReadOnlyList<FieldInfo> VariableReferenceFields => variableReferenceFields;

        protected override void OnFieldAdded(FieldInfo fieldInfo)
        {
            if (typeof(IVariableReference).IsAssignableFrom(fieldInfo.FieldType))
                variableReferenceFields.Add(fieldInfo);
        }
    }
}