#region

using System.Collections.Generic;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationEditorListView : MainBasicListView<ConsiderationEditorListViewModel,
        ConsiderationEditorViewModel, ConsiderationTabSubView>
    {
        public ConsiderationEditorListView() : base(false, false)
        {
        }

        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new ConsiderationEditorNameItemView(this);
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