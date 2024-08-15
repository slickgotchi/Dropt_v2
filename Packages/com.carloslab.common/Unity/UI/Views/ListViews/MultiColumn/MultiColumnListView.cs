#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class
        MultiColumnListView<TListViewModel, TItemViewModel, TRootView> : BaseListView<TListViewModel,
        TItemViewModel, MultiColumnListView, TRootView>
        where TListViewModel : class, IRootViewModelMember, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IRootViewModelMember, IItemViewModel
        where TRootView: BaseView, IRootView
    {
        protected MultiColumnListView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        protected sealed override void InitListView(MultiColumnListView listView)
        {
            RegisterColumns(listView);
            base.InitListView(listView);
        }

        protected virtual void RegisterColumns(MultiColumnListView listView)
        {
            
        }

        protected Column RegisterColumn(MultiColumnListView listView, string columnName, string titleName)
        {
            Column column = CreateColumn(columnName, titleName);
            listView.columns.Add(column);
            return column;
        }

        protected Column RegisterColumn(MultiColumnListView listView, string columnName, string titleName, int index)
        {
            Column column = CreateColumn(columnName, titleName);
            listView.columns.Insert(index, column);
            return column;
        }

        private Column CreateColumn(string columnName, string titleName)
        {
            Column column = new()
            {
                name = columnName,
                title = titleName,
                makeCell = () => Column_OnMakeCell(columnName),
                destroyCell = (element) => Column_OnDestroyCell(element),
                bindCell = (element, index) => Column_OnBindCell(columnName, element, index)
            };
            return column;
        }
        
        private void Column_OnDestroyCell(VisualElement element)
        {
            if(element is IRootViewMember<TRootView> rootViewMember)
                rootViewMember.RootView = null;
        }

        private VisualElement Column_OnMakeCell(string columnName)
        {
            var cell = OnMakeCell(columnName);
            
            if(cell is IRootViewMember<TRootView> rootViewMember)
                rootViewMember.RootView = RootView;

            return cell;
        }

        private void Column_OnBindCell(string columnName, VisualElement element, int index)
        {
            if (index >= ViewModel.Items.Count || index < 0)
                return;

            OnBindCell(columnName, element, index);
        }


        protected abstract VisualElement OnMakeCell(string columnName);

        protected abstract void OnBindCell(string columnName, VisualElement element, int index);
    }
}