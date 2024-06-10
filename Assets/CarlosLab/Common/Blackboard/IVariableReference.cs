#region

using System;

#endregion

namespace CarlosLab.Common
{
    public interface IVariableReference : ICloneable
    {
        string Name { get; internal set; }
        bool IsBlackboardReference { get; internal set; }

        VariableReferenceType ReferenceType { get; }
        object ValueObject { get; set; }
        Blackboard Blackboard { get; internal set; }
        Type ValueType { get; }
    }
}