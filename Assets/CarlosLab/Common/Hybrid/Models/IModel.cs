namespace CarlosLab.Common
{
    public interface IModel
    {
    }

    public interface IModel<TRuntime> : IModel
    {
        TRuntime Runtime { get; }
    }
}