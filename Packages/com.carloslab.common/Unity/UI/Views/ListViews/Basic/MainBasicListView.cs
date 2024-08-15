using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class MainBasicListView<TListViewModel, TItemViewModel, TSubView, TRootView> :
        MainMultiColumnListView<TListViewModel, TItemViewModel, TSubView, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TSubView : BaseView
        where TRootView: BaseView, IRootView
    {
        
        protected MainBasicListView(string visualAssetPath) : base(visualAssetPath)
        {
        }
        
        #region MultiColumn
        
        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterControlsColumn(listView);
        }
        
        protected override VisualElement OnMakeCell(string columnName)
        {
            VisualElement cell = null;
            switch (columnName)
            {
                case NameColumnName:
                    cell = MakeNameCell();
                    break;
                case ControlsColumnName:
                    cell = MakeControlsCell();
                    break;
            }

            return cell;
        }
        
        protected override void OnBindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case NameColumnName:
                    BindNameCell(element, index);
                    break;
                case ControlsColumnName:
                    BindControlsCell(element, index);
                    break;
            }
        }

        #endregion
        
        #region NameColumn

        private const string NameColumnName = "Name";
        protected virtual string NameColumnTitle => "Name";

        protected void RegisterNameColumn(MultiColumnListView listView)
        {
            Column nameColumn = RegisterColumn(listView, NameColumnName, NameColumnTitle);
            nameColumn.stretchable = true;
        }
        
        protected abstract VisualElement MakeNameCell();
        
        protected void BindNameCell(VisualElement element, int index)
        {
            BaseNameItemView<TItemViewModel, TRootView> itemView = element as BaseNameItemView<TItemViewModel, TRootView>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }
        
        #endregion
        
        #region Controls Column
        
        private const string ControlsColumnName = "Controls";


        protected void RegisterControlsColumn(MultiColumnListView listView)
        {
            RegisterColumn(listView, ControlsColumnName, string.Empty);
        }
        
        protected virtual VisualElement MakeControlsCell()
        {
            if (!IsRuntime)
                return new ControlsItemView<TItemViewModel, TRootView>();
            else
                return new VisualElement();
        }
        
        private void BindControlsCell(VisualElement element, int index)
        {
            ControlsItemView<TItemViewModel, TRootView> itemView = element as ControlsItemView<TItemViewModel, TRootView>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }
        
        #endregion
        

    }
}