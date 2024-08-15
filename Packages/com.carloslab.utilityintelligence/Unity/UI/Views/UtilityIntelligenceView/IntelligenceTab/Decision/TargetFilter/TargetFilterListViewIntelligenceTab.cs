using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListViewIntelligenceTab :
        MainBasicListView<TargetFilterListViewModelDecisionTab, TargetFilterItemViewModelDecisionTab, DecisionSubViewIntelligenceTab>
    {
        protected override void OnInitSubView(DecisionSubViewIntelligenceTab subView)
        {
            SubView.TargetFilterView.Hidden += ClearSelection;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowTargetFilterView(SelectedItem);
            else
                SubView.HideTargetFilterView();
        }

        protected override VisualElement MakeNameCell()
        {
            return new TargetFilterNameItemViewIntelligenceTab();
        }

        protected override VisualElement MakeControlsCell()
        {
            return new VisualElement();
        }
    }
}