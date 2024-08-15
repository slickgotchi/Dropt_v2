namespace CarlosLab.Common
{
    public interface IRootObjectComponent : IRuntimeObject
    {
        bool IsEditorOpening { get; }
        bool IsRuntime { get; }
    }
}