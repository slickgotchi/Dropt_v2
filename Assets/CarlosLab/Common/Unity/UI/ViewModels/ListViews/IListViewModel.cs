#region

using System;
using System.Collections.Generic;

#endregion

namespace CarlosLab.Common.UI
{
    public interface IListViewModel : IViewModel
    {
        int SelectedIndex { get; set; }

        void HandleItemIndexChanged(int sourceIndex, int destIndex);

        // void RemoveItemAt(int index);
        int GetItemIndex(IItemViewModel item);
    }

    public interface IListViewModel<TItemModel, TItemViewModel> : IListViewModelWithViewModel<TItemViewModel>,
        IListViewModelWithModel<TItemModel>
        where TItemModel : class, IModelWithId
        where TItemViewModel : BaseItemViewModel<TItemModel>
    {
    }

    public interface IListViewModelWithModel<TItemModel> : IListViewModel where TItemModel : class
    {
        IReadOnlyList<TItemModel> ItemModels { get; }
    }

    public interface IListViewModelWithViewModel<TItemViewModel> : IListViewModel
        where TItemViewModel : IItemViewModel
    {
        IReadOnlyList<TItemViewModel> Items { get; }

        TItemViewModel SelectedItem { get; }

        event Action<TItemViewModel> ItemAdded;
        event Action<TItemViewModel> ItemRemoved;
        event Action<TItemViewModel> ItemRemoving;

        TItemViewModel CreateItem(Type modelType = null, object itemInfo = null);

        bool TryAddItem(TItemViewModel item);
        bool TryRemoveItem(TItemViewModel item);
        bool TryRenameItem(TItemViewModel item, string newName);
    }
}