namespace CarlosLab.Common
{
    public abstract class Model<TRuntime> : IModel<TRuntime>
    {
        public abstract TRuntime Runtime { get; }
    }
}