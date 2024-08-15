namespace CarlosLab.Common
{
    public interface IModel
    {
        public IDataAsset Asset { get;}
    }

    public interface IModel<TRuntime> : IModel
        where TRuntime : class, IRuntimeObject
    {
        TRuntime Runtime { get; }
    }
}