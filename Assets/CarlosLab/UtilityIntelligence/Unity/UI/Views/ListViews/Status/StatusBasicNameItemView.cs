#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class StatusBasicNameItemView<TItemViewModel>
        : StatusBaseNameItemView<TItemViewModel>
        where TItemViewModel : BaseItemViewModel, IStatusViewModel, INameViewModel
    {
        protected StatusBasicNameItemView(bool enableStatus,
            IListViewWithItem<TItemViewModel> listView) : base(enableStatus, true, listView)
        {
        }

        public sealed override bool UpdateView(TItemViewModel viewModel)
        {
            bool result = base.UpdateView(viewModel);
            if (result) NameLabel.SetDataBinding(nameof(Label.text), nameof(INameViewModel.Name), BindingMode.ToTarget);

            return result;
        }
    }
}