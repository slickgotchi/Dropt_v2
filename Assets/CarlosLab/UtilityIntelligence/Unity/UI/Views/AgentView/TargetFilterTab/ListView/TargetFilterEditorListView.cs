using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterEditorListView : MainBasicListView<TargetFilterEditorListViewModel,
        TargetFilterEditorViewModel, TargetFilterTabSubView>
    {
        public TargetFilterEditorListView() : base(false, false)
        {
        }

        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new TargetFilterEditorNameItemView(this);
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