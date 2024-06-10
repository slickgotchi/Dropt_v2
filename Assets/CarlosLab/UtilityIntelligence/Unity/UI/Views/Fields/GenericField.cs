#region

using System;
using CarlosLab.Common;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class GenericField : ValueField
    {
        public GenericField(Type valueType, bool isDelayed = false, string label = null) : base(valueType, isDelayed,
            label)
        {
        }

        public override VisualElement CreateField(Type valueType, bool isDelayed = false, string label = null)
        {
            VisualElement valueField = null;

            switch (valueType)
            {
                case Type type when typeof(IVariableReference).IsAssignableFrom(type) && type.IsGenericType:
                    VariableReferenceField variableReferenceFieldNew = new VariableReferenceField(label);
                    variableReferenceFieldNew.ValueChanged += RaiseValueChanged;
                    variableReferenceFieldNew.InputApplied += RaiseInputApplied;
                    valueField = variableReferenceFieldNew;
                    break;
                // case Type type when type == typeof(InfluenceCurve):
                //     var influenceCurveField = new InfluenceCurveField(label);
                //     influenceCurveField.RegisterValueChangedCallback(evt =>
                //     {
                //         RaiseValueChanged(evt.newValue);
                //     });
                //     HandleInputApplied(influenceCurveField);
                //     valueField = influenceCurveField;
                //     break;
                default:
                    valueField = base.CreateField(valueType, isDelayed, label);
                    break;
            }

            return valueField;
        }
    }
}