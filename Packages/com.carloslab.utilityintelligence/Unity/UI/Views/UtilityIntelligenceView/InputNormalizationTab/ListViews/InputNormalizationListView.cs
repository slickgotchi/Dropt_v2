using System.Collections.Generic;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationListView : MainBasicListView<InputNormalizationListViewModel, InputNormalizationItemViewModel, InputNormalizationTabSubView>
    {
        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowInputEditorView(SelectedItem);
            else
                SubView.HideInputEditorView();
        }

        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new InputNormalizationNameItemView();
        }

        #endregion
    }
}