#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputValueView : BaseView<ConsiderationViewModel>
    {
        private ValueField valueField;

        public InputValueView() : base(null)
        {
        }

        protected override void OnUpdateView(ConsiderationViewModel viewModel)
        {
            UpdateValueField(viewModel.InputValueType);
        }

        public void UpdateValueField(Type newValueType)
        {
            Clear();
            CreateValueField(newValueType);
        }

        private void CreateValueField(Type valueType)
        {
            valueField = new ValueField(valueType, false, "Value");
            if (valueField.IsValid == false) return;

            if (ViewModel.Asset.IsRuntimeAsset)
            {
                valueField.dataSource = ViewModel.ContextViewModel;
                valueField.SetValueDataBinding(nameof(ConsiderationRuntimeContextViewModel.RawInput), BindingMode.ToTarget);
            }
            else
            {
                valueField.dataSource = ViewModel.RuntimeViewModel;
                valueField.SetValueDataBinding(nameof(ConsiderationRuntimeViewModel.RawInput), BindingMode.TwoWay);
            }

            Add(valueField);
        }

        protected override void OnEnableEditMode()
        {
            valueField?.SetEnabled(true);
        }

        protected override void OnEnableRuntimeMode()
        {
            valueField?.SetEnabled(false);
        }
    }
}