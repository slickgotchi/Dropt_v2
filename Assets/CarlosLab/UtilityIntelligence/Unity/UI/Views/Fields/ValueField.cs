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

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ValueField : VisualElement
    {
        public ValueField(Type valueType, bool isDelayed = false, string label = null)
        {
            valueField = CreateField(valueType, isDelayed, label);
            if (valueField != null)
            {
                Type fieldType = valueField.GetType();
                setValueWithoutNotify = fieldType.GetMethod(nameof(BaseField<object>.SetValueWithoutNotify));
                valueProperty = fieldType.GetProperty(nameof(BaseField<object>.value));
                Add(valueField);
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

        [CreateProperty]
        private readonly VisualElement valueField;

        private readonly MethodInfo setValueWithoutNotify;
        private readonly PropertyInfo valueProperty;

        #endregion

        #region Properties

        public VisualElement ConcreteField => valueField;

        public bool IsValid => valueField != null;

        public object Value
        {
            get => valueProperty?.GetValue(valueField);
            set { valueProperty?.SetValue(valueField, value); }
        }

        #endregion

        #region Value Functions

        public void SetValueWithoutNotify(object value)
        {
            setValueWithoutNotify?.Invoke(valueField, new[] { value });
        }

        public void SetValueDataBinding(string sourcePropertyName, BindingMode bindingMode,
            BindingUpdateTrigger updateTrigger = BindingUpdateTrigger.OnSourceChanged)
        {
            valueField?.SetDataBinding(nameof(BaseField<object>.value), sourcePropertyName, bindingMode, updateTrigger);
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
                    FloatField floatField = new FloatField(label);
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
                    ColorField colorField = new ColorField(label);
                    colorField.RegisterValueChangedCallback(evt => { RaiseValueChanged(evt.newValue); });
                    HandleInputApplied(colorField);
                    valueField = colorField;
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
            valueField.style.marginLeft = 0;
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