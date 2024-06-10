#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public class BasicNameItemView<TItemViewModel> : BaseNameItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel, INameViewModel
    {
        public BasicNameItemView(bool enableRename, IListViewWithItem<TItemViewModel> listView) : base(enableRename,
            listView)
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