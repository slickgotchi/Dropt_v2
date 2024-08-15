#region

using System.Collections.Generic;
using UnityEngine.UIElements;
using MultiColumnListView = UnityEngine.UIElements.MultiColumnListView;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMakerListView
        : MainTargetScoreListView<DecisionMakerListViewModel, DecisionMakerItemViewModel, IntelligenceTabSubView>
    {
        private const string DecisionColumnName = "Decision";

        public DecisionMakerListView()
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }

        protected virtual string DecisionColumnTitle => "Best Decision";

        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterDecisionColumn(listView);
            RegisterScoreColumn(listView);
            RegisterControlsColumn(listView);
        }
        
        protected void RegisterDecisionColumn(MultiColumnListView listView)
        {
            Column column = RegisterColumn(listView, DecisionColumnName, DecisionColumnTitle, 1);
            column.stretchable = true;
        }

        protected override VisualElement OnMakeCell(string columnName)
        {
            VisualElement cell = null;
            switch (columnName)
            {
                case DecisionColumnName:
                    cell = MakeDecisionCell();
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
                case DecisionColumnName:
                    BindDecisionCell(element, index);
                    break;
                default:
                    base.OnBindCell(columnName, element, index);
                    break;
            }
        }

        private void BindDecisionCell(VisualElement element, int index)
        {
            var itemView = element as DecisionMakerBestDecisionItemView;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        private VisualElement MakeDecisionCell()
        {
            return new DecisionMakerBestDecisionItemView();
        }

        protected override VisualElement MakeNameCell()
        {
            return new DecisionMakerNameItemView();
        }

        protected override void BindScoreCell(VisualElement element, int index)
        {
            base.BindScoreCell(element, index);
            element.dataSource = ViewModel.Items[index].ContextViewModel;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowDecisionMakerView(SelectedItem);
            else
                SubView.HideDecisionMakerView();
        }
    }
}