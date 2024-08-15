#region

using System;
using System.Collections.Generic;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.Common.UI.Extensions;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

#endregion

namespace CarlosLab.Common.UI
{
    public class ValueField : VisualElement
    {
        public ValueField(Type valueType, bool isDelayed = false, string label = null)
        {
            valueFieldConcrete = CreateField(valueType, isDelayed, label);
            if (valueFieldConcrete != null)
            {
                Type fieldType = valueFieldConcrete.GetType();
                setValueWithoutNotify = fieldType.GetMethod(nameof(BaseField<object>.SetValueWithoutNotify));
                valueProperty = fieldType.GetProperty(nameof(BaseField<object>.value));
                Add(valueFieldConcrete);
            }
        }

        public event Action<object> ValueChanged;
        public event Action InputApplied;

        protected void RaiseValueChanged(object newValue)
        {
            ValueChanged?.Invoke(newValue);
        }

        protected void RaiseInputApplied()
        {
            InputApplied?.Invoke();
        }

        #region Fields

        private readonly VisualElement valueFieldConcrete;

        private readonly MethodInfo setValueWithoutNotify;
        private readonly PropertyInfo valueProperty;

        #endregion

        #region Properties

        public VisualElement ValueFieldConcrete => valueFieldConcrete;

        public bool IsValid => valueFieldConcrete != null;

        public object Value
        {
            get
            {
                if (valueProperty == null) return null;
                
#if UNITY_EDITOR
                if (valueFieldConcrete is LayerMaskField layerMaskField)
                {
                    LayerMask layerMask = layerMaskField.value;
                    return layerMask;
                }
                else
                {
                    return valueProperty.GetValue(valueFieldConcrete);
                }
#else
                return valueProperty.GetValue(valueFieldConcrete);
#endif
            }
            set
            {
                if (valueProperty == null) return;

#if UNITY_EDITOR
                if (valueFieldConcrete is LayerMaskField)
                {
                    if (value is LayerMask layerMask)
                    {
                        int layerMaskInt = layerMask;
                        valueProperty.SetValue(valueFieldConcrete, layerMaskInt);
                    }
                    else
                    {
                        Debug.LogError("ValueField: SetValue LayerMask Error");
                    }
                }
                else
                {
                    valueProperty.SetValue(valueFieldConcrete, value);
                }
#else
                valueProperty.SetValue(valueFieldConcrete, value);
#endif
            }
        }

        #endregion

        #region Value Functions

        public void SetValueWithoutNotify(object value)
        {
            setValueWithoutNotify?.Invoke(valueFieldConcrete, new[] { value });
        }

        public void SetValueDataBinding(string sourcePropertyName, BindingMode bindingMode,
            BindingUpdateTrigger updateTrigger = BindingUpdateTrigger.OnSourceChanged)
        {
            valueFieldConcrete?.SetDataBinding(nameof(BaseField<object>.value), sourcePropertyName, bindingMode, updateTrigger);
        }

        #endregion

        #region UI Functions

        public virtual VisualElement CreateField(Type valueType, bool isDelayed = false, string label = null)
        {
            VisualElement valueField = null;

            switch (valueType)
            {
                case Type type when type == typeof(int):
                    IntegerField integerField = new IntegerField(label);
                    integerField.isDelayed = isDelayed;
                    integerField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(integerField);
                    valueField = integerField;
                    break;
                case Type type when type == typeof(long):
                    LongField longField = new LongField(label);
                    longField.isDelayed = isDelayed;
                    longField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(longField);
                    valueField = longField;
                    break;
                case Type type when type == typeof(string):
                    TextField textField = new TextField(label);
                    textField.isDelayed = isDelayed;
                    textField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(textField);
                    valueField = textField;
                    break;
                case Type type when type == typeof(bool):
                    Toggle toggleField = new Toggle(label);
                    toggleField.RegisterValueChangedCallback(evt =>
                    {
                        RaiseValueChanged(evt.newValue);
                        RaiseInputApplied();
                    });
                    valueField = toggleField;
                    break;
                case Type type when type == typeof(double):
                    DoubleField doubleField = new DoubleField(label);
                    doubleField.isDelayed = isDelayed;
                    doubleField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(doubleField);
                    valueField = doubleField;
                    break;
                case Type type when type == typeof(float):
                    FloatField floatField = new(label);
                    floatField.isDelayed = isDelayed;
                    floatField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(floatField);
                    valueField = floatField;
                    break;
                case Type type when type == typeof(Float2) || type == typeof(Vector2):
                    Vector2Field vector2Field = new Vector2Field(label);
                    vector2Field.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(vector2Field);
                    valueField = vector2Field;
                    break;
                case Type type when type == typeof(Float3) || type == typeof(Vector3):
                    Vector3Field vector3Field = new Vector3Field(label);
                    vector3Field.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(vector3Field);
                    valueField = vector3Field;
                    break;
                case Type type when type == typeof(Int2) || type == typeof(Vector2Int):
                    Vector2IntField vector2IntField = new Vector2IntField(label);
                    vector2IntField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(vector2IntField);
                    valueField = vector2IntField;
                    break;
                case Type type when type == typeof(Int3) || type == typeof(Vector3Int):
                    Vector3IntField vector3IntField = new Vector3IntField(label);
                    vector3IntField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(vector3IntField);
                    valueField = vector3IntField;
                    break;
#if UNITY_EDITOR
                case Type type when type == typeof(Color):
                    ColorField colorField = new(label);
                    colorField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(colorField);
                    valueField = colorField;
                    break;  
                case Type type when type == typeof(LayerMask):
                    LayerMaskField layerMaskField = new (label);
                    layerMaskField.RegisterValueChangedCallback(evt =>
                    {
                        RaiseValueChanged(evt.newValue);
                        RaiseInputApplied();
                    });
                    valueField = layerMaskField;
                    break;  
#endif
                case Type type when type.IsEnum:
                    Enum defaultValue = (Enum)Enum.ToObject(type, 0);
                    EnumField enumField = new (label, defaultValue);
                    enumField.RegisterValueChangedCallback(evt =>
                    {
                        RaiseValueChanged(evt.newValue);
                        RaiseInputApplied();
                    });
                    valueField = enumField;
                    break;
            }

            return valueField;
        }

        public void ClearMarginLeft()
        {
            valueFieldConcrete.style.marginLeft = 0;
        }

        private void HandleInputApplied(VisualElement field)
        {
            List<VisualElement> inputs = field.Query(className: BaseField<object>.inputUssClassName).ToList();
            foreach (VisualElement input in inputs)
            {
                input.RegisterCallback<FocusOutEvent>(evt =>
                {
                    // Debug.Log($"{field.GetType().Name} FocusOutEvent");

                    InputApplied?.Invoke();
                });
            }

            List<VisualElement> labelElements = field.Query(className: BaseField<object>.labelUssClassName).ToList();
            foreach (VisualElement labelElement in labelElements)
            {
                labelElement.RegisterCallback<MouseUpEvent>(evt =>
                {
                    // Debug.Log($"{field.GetType().Name} Label MouseUpEvent");
                    InputApplied?.Invoke();
                });
            }
        }

        #endregion
    }
}