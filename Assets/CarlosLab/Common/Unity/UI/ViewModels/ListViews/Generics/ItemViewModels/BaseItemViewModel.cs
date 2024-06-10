namespace CarlosLab.Common.UI
{
    public abstract class BaseItemViewModel<TItemModel> : BaseItemViewModel, IViewModelWithModel<TItemModel>
        where TItemModel : class, IModelWithId
    {
        private TItemModel model;

        protected BaseItemViewModel(IDataAsset asset, TItemModel model) : base(asset, model)
        {
            this.model = model;
            
            if(model != null)
                RegisterModelEvents(model);
        }

        public override string ModelId => model.Id;

        public TItemModel Model
        {
            get => model;
            set
            {
                if (model == value) return;
                if (model != null) UnregisterModelEvents(model);

                model = value;

                if (model != null) RegisterModelEvents(model);
                OnModelChanged(model);

                RaiseModelChanged();
            }
        }

        public override object ModelObject => model;

        public override void ResetModel()
        {
            Model = null;
        }

        protected virtual void OnModelChanged(TItemModel newModel)
        {
        }

        protected virtual void RegisterModelEvents(TItemModel model)
        {
        }

        protected virtual void UnregisterModelEvents(TItemModel model)
        {
        }
    }
}