namespace CarlosLab.Common.UI
{
    public abstract class ViewModel<TAsset, TModel> : ViewModel<TModel>, IViewModelWithAsset<TAsset>
        where TAsset : IDataAsset
        where TModel : class
    {
        protected ViewModel(TAsset asset, TModel model) : base(asset, model)
        {
            Asset = asset;
        }

        public new TAsset Asset { get; }
    }
}