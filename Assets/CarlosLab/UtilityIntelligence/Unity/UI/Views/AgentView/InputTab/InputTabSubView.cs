#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputTabSubView : BaseView
    {
        public InputTabSubView() : base(null)
        {
            InputEditorView = new ObjectNameEditorView<InputItemViewModel>();
            InputEditorView.Show(false);
            Add(InputEditorView);
        }

        public ObjectNameEditorView<InputItemViewModel> InputEditorView { get; }

        public void ShowInputEditorView(InputItemViewModel viewModel)
        {
            InputEditorView.Show(true);
            InputEditorView.UpdateView(viewModel);
        }

        public void HideInputEditorView()
        {
            InputEditorView.Show(false);
        }
    }
}