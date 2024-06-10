#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class
        MultiColumnListView<TListViewModel, TItemViewModel> : BaseListView<TListViewModel,
        TItemViewModel, MultiColumnListView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IItemViewModel
    {
        protected MultiColumnListView(string visualAssetPath) : base(visualAssetPath)
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
                makeCell = () => HandleMakeCell(columnName),
                bindCell = (element, index) => HandleBindCell(columnName, element, index)
            };
            return column;
        }

        private VisualElement HandleMakeCell(string columnName)
        {
            return OnMakeCell(columnName);
        }

        private void HandleBindCell(string columnName, VisualElement element, int index)
        {
            if (index >= ViewModel.Items.Count || index < 0)
                return;

            OnBindCell(columnName, element, index);
        }


        protected abstract VisualElement OnMakeCell(string columnName);

        protected abstract void OnBindCell(string columnName, VisualElement element, int index);
    }
}