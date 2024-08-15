using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class NameValueListView<TListViewModel, TItemViewModel, TRootView> :
        BasicListView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, INameViewModel, IValueViewModel, IRootViewModelMember
        where TRootView : BaseView, IRootView
    {
        
        protected NameValueListView() : base(null)
        {
        }

        #region MultiColumns
        
        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterValueColumn(listView);
            RegisterControlsColumn(listView);
        }

        protected override VisualElement OnMakeCell(string columnName)
        {
            VisualElement cell = null;
            switch (columnName)
            {
                case ValueColumnName:
                    cell = MakeValueCell();
                    break;
                default:
                    cell = base.OnMakeCell(columnName);
                    break;
            }

            return cell;
        }

        protected override void OnBindCell(string columnName, VisualElement element, int index)
        {
            switch (columnName)
            {
                case ValueColumnName:
                    BindValueCell(element, index);
                    break;
                default:
                    base.OnBindCell(columnName, element, index);
                    break;
            }
        }

        #endregion

        #region ValueColumn

        private const string ValueColumnName = "Value";

        private void RegisterValueColumn(MultiColumnListView listView)
        {
            Column valueColumn = RegisterColumn(listView, ValueColumnName, ValueColumnName, 1);
            valueColumn.stretchable = true;
        }
        
        protected virtual VisualElement MakeValueCell()
        {
            return new ValueItemView<TItemViewModel, TRootView>();
        }
        
        protected virtual void BindValueCell(VisualElement element, int index)
        {
            ValueItemView<TItemViewModel, TRootView> itemView = element as ValueItemView<TItemViewModel, TRootView>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        #endregion
        
    }
}