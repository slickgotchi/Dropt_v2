#region

using System;
using System.Reflection;
using CarlosLab.Common;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ObjectEditorView<TViewModel> : UtilityIntelligenceViewMember<TViewModel>
        where TViewModel : class, IRootViewModelMember<UtilityIntelligenceViewModel>

    {
        public ObjectEditorView() : base(null)
        {
        }

        public event Action<GenericField> FieldValueChanged;

        protected override void OnUpdateView(TViewModel viewModel)
        {
            if (viewModel.ModelObject is GenericModel model) UpdateView(model);
        }

        protected override void OnModelChanged(IModel newModel)
        {
            if (newModel is GenericModel genericModel)
                UpdateView(genericModel);
        }

        private void UpdateView(GenericModel model)
        {
            Clear();

            foreach (FieldInfo fieldInfo in model.PublicFieldInfos.Values)
            {
                GenericField field = new(fieldInfo.FieldType, false, fieldInfo.Name);
                if (!field.IsValid)
                    continue;

                field.RootView = RootView;
                object runtimeObject = model.RuntimeObject;
                object fieldValue = fieldInfo.GetValue(runtimeObject);
                field.Value = fieldValue;

                field.dataSource = runtimeObject;
                field.SetValueDataBinding(fieldInfo.Name, BindingMode.ToTarget);

                field.ValueChanged += newValue =>
                {
                    model.SetValueRuntime(fieldInfo.Name, field.Value);
                    FieldValueChanged?.Invoke(field);
                };

                field.InputApplied += () =>
                {
                    object fieldValue = field.Value;
                    object newValue = field.Value;
                    if (field.Value is IVariableReference variableReference)
                    {
                        fieldValue = variableReference.Clone();
                        newValue = variableReference.ValueObject;
                    }

                    ViewModel.Record($"ObjectEditorView Field: {fieldInfo.Name} Changed, NewValue: {newValue}",
                        () => { model.SetValueWithoutRuntime(fieldInfo.Name, fieldValue); });
                };

                Add(field);
            }
        }

        // public void Reset()
        // {
        //     Clear();
        //     ViewModel = null;
        // }
    }
}