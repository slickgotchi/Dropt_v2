
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionListViewIntelligenceTab : MainTargetScoreListView<DecisionListViewModelIntelligenceTab, DecisionItemViewModelIntelligenceTab, DecisionMakerSubView>
    {
        public DecisionListViewIntelligenceTab()
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }
        
        protected override string TargetColumnTitle => "Best Target";

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowDecisionView(SelectedItem);
            else
                SubView.HideDecisionView();
        }

        protected override VisualElement MakeNameCell()
        {
            return new DecisionNameItemViewIntelligenceTab();
        }
        
        protected override void BindTargetCell(VisualElement element, int index)
        {
            base.BindTargetCell(element, index);
            element.dataSource = ViewModel.Items[index].DecisionContextViewModel;
        }

        protected override void BindScoreCell(VisualElement element, int index)
        {
            base.BindScoreCell(element, index);
            element.dataSource = ViewModel.Items[index].DecisionContextViewModel;
        }
    }
}
