#region

using System;

#endregion

namespace CarlosLab.Common
{
    public interface IDataAsset
    {
        string Name { get; }
        int DataVersion { get; }
        bool IsRuntimeAsset { get; set; }
        bool IsEditorOpening { get; set; }

        string FrameworkVersion { get; }
        string SerializedModel { get; }
        string FormattedSerializedModel { get; }
        object ModelObject { get; }
        IRootViewModel ViewModel { get; }
        bool BlockRecording { get; }
        bool IsInUndoRedo { get; }
        void Record(string name, Action action, bool save = false);
        void SerializeModel();
        void DeserializeModel();
        void ClearModel();
        void Save();
        void ImportModel(string serializedModel);
    }

    public interface IDataAsset<TRuntime> : IDataAsset
        where TRuntime : class
    {
        TRuntime Runtime { get; }
    }
}