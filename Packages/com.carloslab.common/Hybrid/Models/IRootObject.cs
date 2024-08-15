namespace CarlosLab.Common
{
    public interface IRootObject : IRootObjectComponent
    {
        bool IsRuntimeEditorOpening { get; }
        int EditorOpeningCount { get; }
    }
}