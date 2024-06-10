namespace CarlosLab.Common.UI
{
    public abstract class BaseItemViewModel : ViewModel, IItemViewModel
    {
        protected BaseItemViewModel(IDataAsset asset, object model) : base(asset, model)
        {
        }

        public abstract string ModelId { get; }

        public int Index => BaseListViewModel?.GetItemIndex(this) ?? -1;
        public bool IsInList => BaseListViewModel != null;

        public virtual IListViewModel BaseListViewModel { get; protected set; }

        public void HandleItemAdded(IListViewModel listViewModel)
        {
            BaseListViewModel = listViewModel;
        }

        public void HandleItemRemoved()
        {
            BaseListViewModel = null;
        }
    }
}