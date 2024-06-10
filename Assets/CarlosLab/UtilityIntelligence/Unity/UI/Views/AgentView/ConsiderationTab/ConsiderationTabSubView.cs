#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationTabSubView : BaseView
    {
        private readonly ConsiderationEditorView considerationEditorView;

        public ConsiderationTabSubView() : base(null)
        {
            considerationEditorView = new ConsiderationEditorView();
            considerationEditorView.Show(false);
            Add(considerationEditorView);
        }

        public void ShowEditorView(ConsiderationEditorViewModel viewModel)
        {
            considerationEditorView.Show(true);
            considerationEditorView.UpdateView(viewModel);
        }

        public void HideEditorView()
        {
            considerationEditorView.Show(false);
        }
    }
}