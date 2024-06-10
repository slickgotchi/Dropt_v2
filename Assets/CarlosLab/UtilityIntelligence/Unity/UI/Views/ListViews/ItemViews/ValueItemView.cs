#region

using System;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ValueItemView<TViewModel> : BaseItemView<TViewModel>
        where TViewModel : class, IItemViewModel, IValueViewModel

    {
        private ValueField valueField;

        public ValueItemView(IListViewWithItem<TViewModel> listView) : base(listView, null)
        {
        }

        protected ValueField ValueField => valueField;

        protected override void OnUpdateView(TViewModel viewModel)
        {
            UpdateValueField(viewModel.ValueType);
        }

        public void UpdateValueField(Type newValueType)
        {
            Clear();
            CreateValueField(newValueType);
        }

        private void CreateValueField(Type valueType)
        {
            valueField = new ValueField(valueType, true);
            if (valueField.IsValid == false) return;

            valueField.style.flexGrow = 1;
            valueField.SetValueDataBinding(nameof(IValueViewModel.ValueObject), BindingMode.TwoWay);

            Add(valueField);
        }
    }
}