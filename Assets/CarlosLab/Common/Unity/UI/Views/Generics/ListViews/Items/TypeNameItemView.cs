namespace CarlosLab.Common.UI
{
    public abstract class TypeNameItemView<TItemViewModel>
        : BaseNameItemView<TItemViewModel>
        where TItemViewModel : ViewModel, ITypeNameViewModel, IItemViewModel

    {
        protected TypeNameItemView(IListViewWithItem<TItemViewModel> listView) : base(false, listView)
        {
        }

        public override bool UpdateView(TItemViewModel viewModel)
        {
            bool result = base.UpdateView(viewModel);
            if (result) NameLabel.text = viewModel.TypeName;

            return result;
        }
    }
}