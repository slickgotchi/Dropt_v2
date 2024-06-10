namespace CarlosLab.Common
{
    public interface IRootViewModel : IViewModel
    {
        bool IsEditorOpening { get; }
        bool IsRuntimeAsset { get; }
    }
}