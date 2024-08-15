namespace CarlosLab.Common
{
    public interface IRootModel : IModel
    {
        int DataVersion { get; set; }
        string FrameworkVersion { get; set; }
        
        bool IsEditorOpening { get; }
        bool IsRuntime { get; set; }
    }

    public interface IRootModel<TRootObject> : IModel<TRootObject>, IRootModel
        where TRootObject : class, IRootObject
    {
        bool IsRuntimeEditorOpening { get; set; }
        int EditorOpeningCount { get; set; }
    }
}