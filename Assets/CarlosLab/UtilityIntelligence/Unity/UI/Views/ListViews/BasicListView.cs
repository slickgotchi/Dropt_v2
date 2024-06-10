#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class BasicListView<TListViewModel, TItemViewModel> :
        MultiColumnListView<TListViewModel, TItemViewModel>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : BaseItemViewModel
    {
        private const string NameColumnName = "Name";
        private const string ControlsColumnName = "Controls";

        public BasicListView() : base(null)
        {
        }

        protected virtual string NameColumnTitle => "Name";

        protected override void InitListView(MultiColumnListView listView)
        {
            Column nameColumn = RegisterColumn(listView, NameColumnName, NameColumnTitle);
            nameColumn.stretchable = true;
            RegisterColumn(listView, ControlsColumnName, string.Empty);
            base.InitListView(listView);
        }

        #region Make Cells

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

        protected abstract VisualElement MakeNameCell();

        protected virtual VisualElement MakeControlsCell()
        {
            return new ControlsItemView<TItemViewModel>(this);
        }

        #endregion

        #region Bind Cells

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

        protected void BindNameCell(VisualElement element, int index)
        {
            BaseNameItemView<TItemViewModel> itemView = element as BaseNameItemView<TItemViewModel>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        private void BindControlsCell(VisualElement element, int index)
        {
            ControlsItemView<TItemViewModel> itemView = element as ControlsItemView<TItemViewModel>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        #endregion
    }
}