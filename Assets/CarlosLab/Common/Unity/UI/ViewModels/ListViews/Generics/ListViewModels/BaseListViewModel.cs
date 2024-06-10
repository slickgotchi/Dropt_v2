using System;
using System.Collections.Generic;
using Unity.Properties;

namespace CarlosLab.Common.UI
{
    public abstract class BaseListViewModel<TItemViewModel> : ViewModel, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IItemViewModel
    {
        protected BaseListViewModel(IDataAsset asset, object model) : base(asset, model)
        {
        }

        #region Selected Index

        private int selectedIndex = -1;

        [CreateProperty]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value)
                    return;

                selectedIndex = value;
            }
        }

        #endregion

        #region Items

        protected readonly List<TItemViewModel> items = new();

        [CreateProperty] public IReadOnlyList<TItemViewModel> Items => items;

        public TItemViewModel SelectedItem
        {
            get
            {
                if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
                    return null;

                return Items[SelectedIndex];
            }
        }

        #endregion

        #region Events

        public event Action<TItemViewModel> ItemAdded;
        public event Action<TItemViewModel> ItemRemoved;
        public event Action<TItemViewModel> ItemRemoving;

        #endregion

        #region Raise Events

        protected void RaiseItemAdded(TItemViewModel item)
        {
            ItemAdded?.Invoke(item);
        }

        protected void RaiseItemRemoving(TItemViewModel item)
        {
            ItemRemoving?.Invoke(item);
        }

        protected void RaiseItemRemoved(TItemViewModel item)
        {
            ItemRemoved?.Invoke(item);
        }

        #endregion

        #region Event Functions

        public void HandleItemIndexChanged(int sourceIndex, int destIndex)
        {
            OnItemIndexChanged(sourceIndex, destIndex);
        }

        protected virtual void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
        }

        #endregion

        #region Item Operations

        public abstract TItemViewModel CreateItem(Type modelType, object itemInfo);

        // public void RemoveItemAt(int index)
        // {
        //     TryRemoveItem(items[index]);
        // }

        public int GetItemIndex(IItemViewModel item)
        {
            TItemViewModel itemViewModel = item as TItemViewModel;
            if (itemViewModel == null)
                return -1;

            return items.IndexOf(itemViewModel);
        }

        public abstract bool TryAddItem(TItemViewModel item);

        public abstract bool TryRemoveItem(TItemViewModel item);

        public virtual bool TryRenameItem(TItemViewModel item, string newName)
        {
            return false;
        }

        #endregion
    }
}