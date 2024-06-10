namespace CarlosLab.Common.UI
{
    public class DeletableItemView<TItemViewModel> : BaseItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel

    {
        public DeletableItemView(IListViewWithItem<TItemViewModel> listView) : base(listView, null)
        {
            CreateRemoveButton();
        }
    }
}