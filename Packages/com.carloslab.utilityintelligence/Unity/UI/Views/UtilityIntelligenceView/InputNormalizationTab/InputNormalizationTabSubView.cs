using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationTabSubView : UtilityIntelligenceViewMember
    {
        public InputNormalizationTabSubView()
        {
            InputNormalizationView = new ();
            InputNormalizationView.Show(false);
            Add(InputNormalizationView);
        }

        public InputNormalizationView InputNormalizationView { get; }

        public void ShowInputEditorView(InputNormalizationItemViewModel viewModel)
        {
            InputNormalizationView.Show(true);
            InputNormalizationView.UpdateView(viewModel);
        }

        public void HideInputEditorView()
        {
            InputNormalizationView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            InputNormalizationView.RootView = rootView;
        }
    }
}