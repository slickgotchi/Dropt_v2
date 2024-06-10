#region

using System.Collections.Generic;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionListView
        : MainBasicListView<DecisionListViewModel, DecisionViewModel, DecisionMakerSubView>
    {
        public DecisionListView() : base(true, true)
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

        #region Make/Bind Cells

        protected override VisualElement MakeNameCell()
        {
            return new DecisionNameItemView(this);
        }

        protected override void BindTargetCell(VisualElement element, int index)
        {
            base.BindTargetCell(element, index);
            element.dataSource = ViewModel.Items[index].ContextViewModel;
        }

        protected override void BindScoreCell(VisualElement element, int index)
        {
            base.BindScoreCell(element, index);
            element.dataSource = ViewModel.Items[index].ContextViewModel;
        }

        #endregion
    }
}