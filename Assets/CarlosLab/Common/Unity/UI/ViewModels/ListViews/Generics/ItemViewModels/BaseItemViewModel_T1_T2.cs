namespace CarlosLab.Common.UI
{
    public abstract class BaseItemViewModel<TItemModel, TListViewModel> : BaseItemViewModel<TItemModel>
        where TListViewModel : class, IListViewModel
        where TItemModel : class, IModelWithId
    {
        private TListViewModel listViewModel;

        protected BaseItemViewModel(IDataAsset asset, TItemModel model) : base(asset, model)
        {
        }

        public override IListViewModel BaseListViewModel
        {
            get => listViewModel;
            protected set => listViewModel = value as TListViewModel;
        }

        public TListViewModel ListViewModel => listViewModel;
    }
}