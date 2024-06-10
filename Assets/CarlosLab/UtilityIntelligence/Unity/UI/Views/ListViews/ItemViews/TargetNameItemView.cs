#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetNameItemView<TItemViewModel> : BaseNameItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel
    {
        public TargetNameItemView(IListViewWithItem<TItemViewModel> listView) : base(false, listView)
        {
        }

        protected override void OnUpdateView(TItemViewModel viewModel)
        {
            NameLabel.SetDataBinding(nameof(Label.text), nameof(ITargetViewModel.TargetName), BindingMode.ToTarget);
        }
    }
}