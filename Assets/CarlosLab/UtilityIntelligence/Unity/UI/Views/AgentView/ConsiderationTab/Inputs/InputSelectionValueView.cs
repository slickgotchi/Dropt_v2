#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputSelectionValueView : BaseView<ConsiderationRuntimeViewModel>
    {
        private ValueField valueField;

        public InputSelectionValueView() : base(null)
        {
        }

        protected override void OnUpdateView(ConsiderationRuntimeViewModel runtimeViewModel)
        {
            UpdateValueField(runtimeViewModel.EditorViewModel);
        }

        public void UpdateValueField(ConsiderationEditorViewModel editorViewModel)
        {
            Clear();
            
            if(editorViewModel.Input != null)
                CreateValueField(editorViewModel.InputValueType);
        }

        private void CreateValueField(Type valueType)
        {
            valueField = new ValueField(valueType, false, "Value");
            if (valueField.IsValid == false) return;

            valueField.style.flexGrow = 1;
            
            valueField.SetValueDataBinding(nameof(ConsiderationRuntimeViewModel.RawInput), BindingMode.TwoWay);

            if (ViewModel.Asset.IsRuntimeAsset)
            {
                OnEnableRuntimeMode();
            }
            else
            {
                OnEnableEditMode();
            }

            Add(valueField);
        }

        protected override void OnEnableEditMode()
        {
            if (valueField != null)
                valueField.SetEnabled(true);
        }

        protected override void OnEnableRuntimeMode()
        {
            if (valueField != null)
                valueField.SetEnabled(false);
        }
    }
}