#region

using System;
using System.Collections.Generic;
using System.Linq;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class NormalizationSelectionView : BaseView<NormalizationCacheViewModel>
    {
        private PopupField<Type> normalizationField;
        private FloatField normalizedInputField;
        private ObjectEditorView<NormalizationViewModel> paramsEditorView;

        public NormalizationSelectionView() : base(null)
        {
            CreateNormalizationField();
        }

        private void CreateNormalizationField()
        {
            Foldout foldout = new();
            foldout.text = "Input Normalization";
            Add(foldout);

            normalizationField = new PopupField<Type>();
            normalizationField.label = "Type";
            FormatNormalizationField();
            HandleNormalizationFieldValueChanged();
            foldout.Add(normalizationField);

            paramsEditorView = new ObjectEditorView<NormalizationViewModel>();
            // paramsEditorView.style.marginLeft = -4;
            paramsEditorView.FieldValueChanged += _ => ViewModel.EditorViewModel.CalculateScore();
            foldout.Add(paramsEditorView);


            normalizedInputField = new FloatField("Normalized Input");
            normalizedInputField.SetEnabled(false);
            foldout.Add(normalizedInputField);
        }

        protected override void OnUpdateView(NormalizationCacheViewModel viewModel)
        {
            normalizedInputField.dataSource = viewModel.EditorViewModel.RuntimeViewModel;
            normalizedInputField.SetDataBinding(nameof(FloatField.value),
                nameof(ConsiderationRuntimeViewModel.NormalizedInput), BindingMode.ToTarget);
            UpdateNormalizedInputFieldDisplay();
        }

        public void UpdateNormalizationField(Type inputValueType)
        {
            Type normalizationType = GetNormalizationType(inputValueType);
            UpdateNormalizationFieldChoices(normalizationType);
        }

        public void Reset()
        {
            normalizationField.value = null;
            normalizationField.choices.Clear();
        }

        public void ResetWithoutNotify()
        {
            normalizationField.SetValueWithoutNotify(null);
            normalizationField.choices.Clear();
            paramsEditorView.Reset();
        }

        private void UpdateNormalizationFieldChoices(Type normalizationType)
        {
            normalizationField.SetValueWithoutNotify(null);
            normalizationField.choices.Clear();

            List<Type> types = null;
#if UNITY_EDITOR
            types = TypeCache.GetTypesDerivedFrom(normalizationType).ToList();
#else
            types = new();
#endif
            
            types.Sort((a, b) =>
            {
                string aName = a.Name.Replace("Normalization", string.Empty);
                string bName = b.Name.Replace("Normalization", string.Empty);
                return string.Compare(aName, bName, StringComparison.Ordinal);
            });
            
            normalizationField.choices.Add(null);
            foreach (Type type in types)
            {
                if (type.IsGenericType || type.IsAbstract) continue;

                normalizationField.choices.Add(type);
            }

            if (normalizationField.choices.Count > 0)
            {
                if (ViewModel.CurrentNormalization == null ||
                    !normalizationType.IsAssignableFrom(ViewModel.CurrentNormalization.RuntimeType))
                    normalizationField.value = normalizationField.choices[0];

                if (ViewModel.CurrentNormalization != null && normalizationField.value == null)
                {
                    Type runtimeType = ViewModel.CurrentNormalization.RuntimeType;
                    normalizationField.value = runtimeType;
                }
            }
        }

        private void FormatNormalizationField()
        {
            normalizationField.formatListItemCallback = FormatItemUsingTypeName;
            normalizationField.formatSelectedValueCallback = FormatItemUsingTypeName;
            
            string FormatItemUsingTypeName(Type type)
            {
                if (type == null)
                    return "None";

                return type.Name;
            }
        }

        private void HandleNormalizationFieldValueChanged()
        {
            normalizationField.RegisterValueChangedCallback(evt =>
            {
                Type normalizationType = evt.newValue;

                if (normalizationType != null)
                {
                    Type inputValueType = GetInputValueType(normalizationType, typeof(InputNormalization<>));

                    ViewModel.UpdateNormalizationModel(normalizationType, inputValueType);

                    paramsEditorView.UpdateView(ViewModel.CurrentNormalization);
                }
                else
                {
                    // ViewModel.CurrentNormalization = null;
                    paramsEditorView.Reset();
                }

                UpdateNormalizedInputFieldDisplay();
            });
        }

        private void UpdateNormalizedInputFieldDisplay()
        {
            if (ViewModel.CurrentNormalization == null)
                normalizedInputField.SetDisplay(false);
            else
                normalizedInputField.SetDisplay(true);
        }

        private Type GetInputValueType(Type normalizationType, Type normalizationGenericType)
        {
            Type inputValueType = null;
            while (normalizationType.BaseType != null)
            {
                normalizationType = normalizationType.BaseType;
                if (normalizationType.IsGenericType && normalizationType.GetGenericTypeDefinition() == normalizationGenericType)
                {
                    inputValueType = normalizationType.GetGenericArguments()[0];
                    break;
                }
            }

            return inputValueType;
        }

        private Type GetNormalizationType(Type inputValueType)
        {
            Type normalizationType = typeof(InputNormalization<>).MakeGenericType(inputValueType);
            return normalizationType;
        }
    }
}