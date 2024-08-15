using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListView : MainBasicListView<TargetFilterListViewModel,
        TargetFilterItemViewModel, TargetFilterTabSubView>
    {
        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new TargetFilterNameItemView();
        }

        #endregion

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowEditorView(SelectedItem);
            else
                SubView.HideEditorView();
        }
    }
}