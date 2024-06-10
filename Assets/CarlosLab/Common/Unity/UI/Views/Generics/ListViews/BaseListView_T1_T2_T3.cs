#region

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class BaseListView<TListViewModel, TItemViewModel, TListView> :
        BaseListView<TListViewModel, TListView>
        , IListViewWithItem<TItemViewModel>
        where TListView : BaseListView, new()
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IItemViewModel
    {
        #region Constructors

        public BaseListView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        #endregion

        #region Properties

        public TItemViewModel SelectedItem => (TItemViewModel)ListView.selectedItem;

        #endregion

        #region Views

        public sealed override bool UpdateView(TListViewModel viewModel)
        {
            if (!ValidateViewModel(viewModel))
                return false;

            ListView.SetBinding("listview-custom-binding", new ListViewCustomBinding
            {
                dataSourcePath = PropertyPath.FromName(nameof(IListViewModelWithViewModel<TItemViewModel>.Items))
            });

            return base.UpdateView(viewModel);
        }

        #endregion

        #region Events

        public event Action<TItemViewModel> ItemAdded;
        public event Action<TItemViewModel> ItemRemoved;

        #endregion

        #region Event Handlers

        protected override void OnRegisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded += HandleItemAdded;
            viewModel.ItemRemoving += HandleItemRemoving;
            viewModel.ItemRemoved += HandleItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded -= HandleItemAdded;
            viewModel.ItemRemoving -= HandleItemRemoving;
            viewModel.ItemRemoved -= HandleItemRemoved;
        }

        private void HandleItemAdded(TItemViewModel newItem)
        {
            if (newItem.Index == SelectedIndex)
                SelectedIndex = SelectedItem.Index;

            ItemAdded?.Invoke(newItem);

            OnItemAdded(newItem);
        }

        protected virtual void OnItemAdded(TItemViewModel newItem)
        {
        }

        private void HandleItemRemoved(TItemViewModel item)
        {
            ItemRemoved?.Invoke(item);
            OnItemRemoved(item);
        }

        protected virtual void OnItemRemoved(TItemViewModel item)
        {
        }

        private void HandleItemRemoving(TItemViewModel item)
        {
            if (item.Index == SelectedIndex)
                ClearSelection();
        }

        public bool TryAddItem(TItemViewModel item)
        {
            return ViewModel.TryAddItem(item);
        }

        public bool TryRenameItem(TItemViewModel item, string newName)
        {
            return ViewModel.TryRenameItem(item, newName);
        }

        public bool TryRemoveItem(TItemViewModel item)
        {
            return ViewModel.TryRemoveItem(item);
        }

        #endregion
    }
}