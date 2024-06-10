#region

using System.Collections.Generic;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListView :
        MainBasicListView<TargetFilterListViewModel, TargetFilterViewModel, DecisionSubView>
    {
        private TargetFilterEditorListViewModel editorViewModel;

        public TargetFilterListView() : base(false, false)
        {
        }
        
        private TargetFilterEditorListViewModel EditorViewModel
        {
            get => editorViewModel;
            set
            {
                if (editorViewModel == value)
                    return;

                editorViewModel = value;
            }
        }

        protected override void OnInitSubView(DecisionSubView subView)
        {
            SubView.TargetFilterView.Hidden += ClearSelection;
        }
        
        protected override void OnUpdateView(TargetFilterListViewModel viewModel)
        {
            EditorViewModel = UtilityIntelligenceEditorUtils.TargetFilters;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowTargetFilterEditorView(SelectedItem);
            else
                SubView.HideTargetFilterEditorView();
        }

        protected override VisualElement MakeNameCell()
        {
            return new TargetFilterNameItemView(this);
        }
    }
}