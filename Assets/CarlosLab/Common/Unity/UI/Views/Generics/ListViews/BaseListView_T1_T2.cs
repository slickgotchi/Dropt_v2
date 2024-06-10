using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class BaseListView<TListViewModel, TListView> : BaseView<TListViewModel>,
        IListViewWithList<TListViewModel>
        where TListViewModel : class, IListViewModel
        where TListView : BaseListView, new()
    {
        protected BaseListView(string visualAssetPath) : base(visualAssetPath)
        {
            InitListView(visualAssetPath);
            RegisterListViewEvents();
            SelectionType = SelectionType.Single;
            VirtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            ReorderMode = ListViewReorderMode.Animated;
            Reorderable = true;
            Reorderable = false;
        }

        #region Properties

        protected TListView ListView { get; private set; }

        #endregion

        #region ListView's Events

        public event Action<IList> ItemsSourceChanged;

        #endregion

        private void InitListView(string visualAssetPath)
        {
            if (string.IsNullOrEmpty(visualAssetPath))
            {
                ListView = new TListView();
                Add(ListView);
            }
            else
                ListView = this.Q<TListView>();

            InitListView(ListView);
        }

        #region ListView Selection

        public void ClearSelection()
        {
            SelectedIndex = -10;
        }

        #endregion

        #region ListView's Properties

        public int SelectedIndex
        {
            get => ListView.selectedIndex;
            set
            {
                if (ListView.selectedIndex == value)
                    return;

                ListView.selectedIndex = value;
                OnSelectedIndexChanged(value);
            }
        }

        public IList Items => ListView.itemsSource;

        public float FixedItemHeight
        {
            get => ListView.fixedItemHeight;
            set => ListView.fixedItemHeight = value;
        }

        public SelectionType SelectionType
        {
            get => ListView.selectionType;
            set => ListView.selectionType = value;
        }

        public CollectionVirtualizationMethod VirtualizationMethod
        {
            get => ListView.virtualizationMethod;
            set => ListView.virtualizationMethod = value;
        }

        public bool Reorderable
        {
            get => ListView.reorderable;
            set => ListView.reorderable = value;
        }

        public ListViewReorderMode ReorderMode
        {
            get => ListView.reorderMode;
            set => ListView.reorderMode = value;
        }

        #endregion

        #region Views

        protected virtual void InitListView(TListView listView)
        {
            OnInitListView(listView);
        }

        protected virtual void OnInitListView(TListView listView)
        {
        }

        #endregion

        #region ListView Events

        private void RegisterListViewEvents()
        {
            ListView.itemsSourceChanged += HandleItemsSourceChanged;
            ListView.selectionChanged += HandleSelectionChanged;
            ListView.itemIndexChanged += HandleItemIndexChanged;
            ListView.itemsChosen += HandleItemChosen;
            ListView.selectedIndicesChanged += HandleSelectedIndicesChanged;
        }

        private void HandleItemsSourceChanged()
        {
            CommonConsole.Instance.Log(
                $"{GetType().Name} RaiseItemsSourceChanged SelectedIndex: {SelectedIndex} ModelSelectedIndex: {ViewModel.SelectedIndex}");
            ItemsSourceChanged?.Invoke(Items);

            OnItemsSourceChanged();
        }

        private void HandleItemChosen(IEnumerable<object> items)
        {
            CommonConsole.Instance.Log($"{GetType().Name} RaiseItemChosen");
        }

        private void HandleItemIndexChanged(int sourceIndex, int destIndex)
        {
            CommonConsole.Instance.Log($"{GetType().Name} HandleItemIndexChanged {sourceIndex} {destIndex}");

            ViewModel.HandleItemIndexChanged(sourceIndex, destIndex);
            OnItemIndexChanged(sourceIndex, destIndex);
        }

        private void HandleSelectionChanged(IEnumerable<object> items)
        {
            ViewModel.SelectedIndex = SelectedIndex;
            CommonConsole.Instance.Log($"{GetType().Name} HandleSelectionChanged SelectedIndex: {SelectedIndex}");

            OnSelectionChanged(items);
        }

        private void HandleSelectedIndicesChanged(IEnumerable<int> items)
        {
            CommonConsole.Instance.Log($"{GetType().Name} RaiseSelectedIndicesChanged");
        }

        #endregion

        #region Event Functions

        protected virtual void OnSelectedIndexChanged(int index)
        {
        }

        protected virtual void OnItemsSourceChanged()
        {
        }


        protected virtual void OnSelectionChanged(IEnumerable<object> items)
        {
        }


        protected virtual void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
        }

        #endregion
    }
}