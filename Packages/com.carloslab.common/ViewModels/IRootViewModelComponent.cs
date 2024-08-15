using System;

namespace CarlosLab.Common
{
    public interface IRootViewModelComponent : IViewModel
    {
        int DataVersion { get;}
        string FrameworkVersion { get;}
        bool IsEditorOpening { get; }
        bool IsRuntime { get; }
        void Record(string name, Action action, bool save = false);
    }
}