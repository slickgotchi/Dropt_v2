#region

using System.Collections.Generic;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationListView
        : MainBasicListView<ConsiderationListViewModel, ConsiderationViewModel, DecisionSubView>
    {
        private ConsiderationEditorListViewModel editorViewModel;


        // protected override string ScoreColumnTitle => "Compensated Score";
        //
        // protected override float ScoreColumnWidth => 130;

        public ConsiderationListView() : base(true, true)
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }

        private ConsiderationEditorListViewModel EditorViewModel
        {
            get => editorViewModel;
            set
            {
                if (editorViewModel == value)
                    return;

                editorViewModel = value;
            }
        }

        protected override void InitListView(MultiColumnListView listView)
        {
            listView.style.marginBottom = 10;
        }

        protected override void OnInitSubView(DecisionSubView subView)
        {
            SubView.ConsiderationView.Hidden += ClearSelection;
        }

        protected override void OnUpdateView(ConsiderationListViewModel viewModel)
        {
            EditorViewModel = UtilityIntelligenceEditorUtils.Considerations;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowConsiderationView(SelectedItem);
            else
                SubView.HideConsiderationView();
        }

        protected override VisualElement MakeNameCell()
        {
            return new ConsiderationNameItemView(this);
        }

        protected override void BindTargetCell(VisualElement element, int index)
        {
            base.BindTargetCell(element, index);
            element.dataSource = ViewModel.Items[index].ContextViewModel;
        }

        protected override VisualElement MakeScoreCell()
        {
            ConsiderationScoreItemView scoreCell = new(this);
            scoreCell.tooltip = "Compensated Score";
            return scoreCell;
        }

        protected override void BindScoreCell(VisualElement element, int index)
        {
            base.BindScoreCell(element, index);
            element.dataSource = ViewModel.Items[index].ContextViewModel;
        }
    }
}