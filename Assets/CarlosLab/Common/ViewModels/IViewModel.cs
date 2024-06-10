#region

using System;

#endregion

namespace CarlosLab.Common
{
    public interface IViewModel
    {
        string Id { get; }
        IDataAsset Asset { get; }
        object ModelObject { get; }
        void ResetModel();
        event Action ModelChanged;
    }

    public interface IViewModelWithAsset<TAsset> : IViewModel
        where TAsset : IDataAsset
    {
        new TAsset Asset { get; }
    }

    public interface IViewModelWithModel<TModel> : IViewModel
    {
        TModel Model { get; set; }
    }
}