#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ValueEditorView<TViewModel> : BaseView<TViewModel>
        where TViewModel : class, IValueViewModel
    {
        private ValueField valueField;

        public ValueEditorView() : base(null)
        {
        }

        protected override void OnUpdateView(TViewModel viewModel)
        {
            Clear();
            valueField = new ValueField(viewModel.ValueType, false, "Value");
            if (valueField.IsValid == false) return;

            valueField.SetDataBinding(nameof(ValueField.Value), nameof(IValueViewModel.ValueObject),
                BindingMode.TwoWay);
            Add(valueField);
        }
    }
}