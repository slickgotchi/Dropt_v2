#region

using System.Collections.Generic;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationListView : MainBasicListView<ConsiderationListViewModel,
        ConsiderationItemViewModel, ConsiderationTabSubView>
    {

        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new ConsiderationNameItemView();
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