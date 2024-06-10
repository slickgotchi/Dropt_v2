namespace CarlosLab.Common.UI
{
    public abstract class ViewModel<TModel> : ViewModel, IViewModelWithModel<TModel> where TModel : class
    {
        private TModel model;

        public ViewModel(IDataAsset asset, TModel model) : base(asset,
            model)
        {
            this.model = model;
            OnRegisterModelEvents(model);
        }

        public TModel Model
        {
            get => model;
            set
            {
                if (model == value) return;
                if (model != null) OnUnregisterModelEvents(model);

                model = value;

                if (model != null) OnRegisterModelEvents(model);
                OnModelChanged(model);

                RaiseModelChanged();
            }
        }

        public override object ModelObject => model;

        public override void ResetModel()
        {
            Model = null;
        }

        protected virtual void OnModelChanged(TModel newModel)
        {
        }

        protected virtual void OnRegisterModelEvents(TModel model)
        {
        }

        protected virtual void OnUnregisterModelEvents(TModel model)
        {
        }
    }
}