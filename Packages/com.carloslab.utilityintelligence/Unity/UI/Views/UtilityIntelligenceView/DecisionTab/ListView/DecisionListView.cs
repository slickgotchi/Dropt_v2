#region

using System.Collections.Generic;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionListView
        : MainBasicListView<DecisionListViewModel, DecisionItemViewModel, DecisionTabSubView>
    {
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
            return new DecisionNameItemView();
        }

        #endregion
    }
}