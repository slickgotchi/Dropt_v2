#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class StatusTypeNameItemView<TItemViewModel>
        : StatusBaseNameItemView<TItemViewModel>
        where TItemViewModel : BaseItemViewModel, ITypeNameViewModel, IStatusViewModel

    {
        protected StatusTypeNameItemView(bool enableStatus,
            IListViewWithItem<TItemViewModel> listView) : base(enableStatus, false, listView)
        {
        }

        public sealed override bool UpdateView(TItemViewModel viewModel)
        {
            bool result = base.UpdateView(viewModel);
            if (result) NameLabel.text = viewModel.TypeName;

            return result;
        }
    }
}