#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class NameValueListView<TListViewModel, TItemViewModel> :
        BasicListView<TListViewModel, TItemViewModel>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : BaseItemViewModel, INameViewModel, IValueViewModel
    {
        private const string ValueColumnName = "Value";

        protected sealed override void InitListView(MultiColumnListView listView)
        {
            base.InitListView(listView);
            Column valueColumn = RegisterColumn(listView, ValueColumnName, ValueColumnName, 1);
            valueColumn.stretchable = true;
        }

        #region Make Cells

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

        protected virtual VisualElement MakeValueCell()
        {
            return new ValueItemView<TItemViewModel>(this);
        }

        #endregion

        #region Bind Cells

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

        protected virtual void BindValueCell(VisualElement element, int index)
        {
            ValueItemView<TItemViewModel> itemView = element as ValueItemView<TItemViewModel>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        #endregion
    }
}