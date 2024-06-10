#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionSubView : BaseView
    {
        public DecisionSubView() : base(null)
        {
            TargetFilterView = new();
            TargetFilterView.Show(false);
            Add(TargetFilterView);

            ActionEditorView = new();
            ActionEditorView.Show(false);
            Add(ActionEditorView);

            ConsiderationView = new();
            ConsiderationView.Show(false);
            Add(ConsiderationView);
        }

        public TargetFilterView TargetFilterView { get; }
        public ObjectNameEditorView<ActionViewModel> ActionEditorView { get; }

        public ConsiderationView ConsiderationView { get; }

        public void ShowTargetFilterEditorView(TargetFilterViewModel viewModel)
        {
            HideAllExcept(TargetFilterView);
            TargetFilterView.Show(true);
            TargetFilterView.UpdateView(viewModel);
        }

        public void HideTargetFilterEditorView()
        {
            TargetFilterView.Show(false);
        }

        public void ShowActionEditorView(ActionViewModel viewModel)
        {
            HideAllExcept(ActionEditorView);
            ActionEditorView.Show(true);
            ActionEditorView.UpdateView(viewModel);
        }

        public void HideActionEditorView()
        {
            ActionEditorView.Show(false);
        }

        public void ShowConsiderationView(ConsiderationViewModel viewModel)
        {
            HideAllExcept(ConsiderationView);
            ConsiderationView.Show(true);
            ConsiderationView.UpdateView(viewModel);
        }

        public void HideConsiderationView()
        {
            ConsiderationView.Show(false);
        }

        private void HideAllExcept(VisualElement view)
        {
            if (TargetFilterView != view)
                HideTargetFilterEditorView();

            if (ActionEditorView != view)
                HideActionEditorView();

            if (ConsiderationView != view)
                HideConsiderationView();
        }
    }
}