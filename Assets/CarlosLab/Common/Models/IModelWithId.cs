namespace CarlosLab.Common
{
    public interface IModelWithId : IModel
    {
        string Id { get; internal set; }
    }
}