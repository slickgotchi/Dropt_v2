namespace CarlosLab.Common
{
    public interface TRootModel : IModel
    {
        int DataVersion { get; protected internal set; }
        string FrameworkVersion { get; protected internal set; }
        
        bool IsEditorOpening { get; protected internal set; }
        bool IsRuntimeAsset { get; protected internal set; }
    }
}