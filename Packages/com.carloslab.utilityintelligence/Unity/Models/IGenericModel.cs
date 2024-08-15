using System.Collections.Generic;
using System.Reflection;
using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public interface IGenericModel : IModel
    {
        // public IReadOnlyList<FieldInfo> VariableReferenceFields { get; }
        object GetValue(string fieldName);
        void SetValue(string fieldName, object value);
        void SetVariableReference(string oldVariableName, string newVariableName);
        void SetVariableReference(Blackboard blackboard);
    }
}