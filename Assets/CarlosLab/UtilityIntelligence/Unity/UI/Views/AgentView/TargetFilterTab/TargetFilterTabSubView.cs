using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterTabSubView : BaseView
    {
        public ObjectNameEditorView<TargetFilterEditorViewModel> EditorView { get; }
        public TargetFilterTabSubView() : base(null)
        {
            EditorView = new ObjectNameEditorView<TargetFilterEditorViewModel>();
            EditorView.Show(false);
            Add(EditorView);
        }


        public void ShowEditorView(TargetFilterEditorViewModel viewModel)
        {
            EditorView.Show(true);
            EditorView.UpdateView(viewModel);
        }

        public void HideEditorView()
        {
            EditorView.Show(false);
        }
    }
}