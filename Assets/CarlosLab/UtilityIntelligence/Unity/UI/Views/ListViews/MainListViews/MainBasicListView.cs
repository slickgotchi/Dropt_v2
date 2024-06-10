#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class MainBasicListView<TListViewModel, TItemViewModel, TSubView> :
        MainMultiColumnListView<TListViewModel, TItemViewModel, TSubView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : BaseItemViewModel
        where TSubView : BaseView
    {
        private const string NameColumnName = "Name";

        private const string ScoreColumnName = "Score";

        private const string TargetColumnName = "Target";

        private const string ControlsColumnName = "Controls";

        private readonly bool hasScoreColumn;
        private readonly bool hasTargetColumn;

        public MainBasicListView(bool hasScoreColumn, bool hasTargetColumn) : base(null)
        {
            this.hasScoreColumn = hasScoreColumn;
            this.hasTargetColumn = hasTargetColumn;

            RegisterColumns(ListView);
        }

        protected virtual string NameColumnTitle => "Name";
        protected virtual string TargetColumnTitle => "Target";
        
        protected virtual string ScoreColumnTitle => "Score";

        protected virtual float ScoreColumnWidth => 50;

        private void RegisterColumns(MultiColumnListView listView)
        {
            Column nameColumn = RegisterColumn(listView, NameColumnName, NameColumnTitle);
            nameColumn.stretchable = true;

            if (hasTargetColumn)
            {
                Column targetColumn = RegisterColumn(listView, TargetColumnName, TargetColumnTitle);
                targetColumn.stretchable = true;
            }

            if (hasScoreColumn)
            {
                Column scoreColumn = RegisterColumn(listView, ScoreColumnName, ScoreColumnTitle);
                scoreColumn.width = ScoreColumnWidth;
            }

            Column controlsColumn = RegisterColumn(listView, ControlsColumnName, string.Empty);

            OnColumnsRegistered(listView);
        }

        protected virtual void OnColumnsRegistered(MultiColumnListView listView)
        {
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
                case ScoreColumnName:
                    cell = MakeScoreCell();
                    break;
                case TargetColumnName:
                    cell = MakeTargetCell();
                    break;
                case ControlsColumnName:
                    cell = MakeControlsCell();
                    break;
            }

            return cell;
        }

        protected abstract VisualElement MakeNameCell();

        protected virtual VisualElement MakeScoreCell()
        {
            return new ScoreItemView<TItemViewModel>(this);
        }

        protected virtual VisualElement MakeTargetCell()
        {
            return new TargetNameItemView<TItemViewModel>(this);
        }

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
                case ScoreColumnName:
                    BindScoreCell(element, index);
                    break;
                case TargetColumnName:
                    BindTargetCell(element, index);
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

        protected virtual void BindScoreCell(VisualElement element, int index)
        {
            ScoreItemView<TItemViewModel> itemView = element as ScoreItemView<TItemViewModel>;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        protected virtual void BindTargetCell(VisualElement element, int index)
        {
            TargetNameItemView<TItemViewModel> itemView = element as TargetNameItemView<TItemViewModel>;
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