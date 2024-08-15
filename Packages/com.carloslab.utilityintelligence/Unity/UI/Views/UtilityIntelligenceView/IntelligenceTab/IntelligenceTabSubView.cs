#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class IntelligenceTabSubView : UtilityIntelligenceViewMember
    {
        private readonly DecisionMakerSplitView decisionMakerView;

        public IntelligenceTabSubView()
        {
            decisionMakerView = new DecisionMakerSplitView();
            decisionMakerView.Show(false);

            Add(decisionMakerView);
        }

        public void ShowDecisionMakerView(DecisionMakerItemViewModel viewModel)
        {
            decisionMakerView.Show(true);
            decisionMakerView.MainView.UpdateView(viewModel);
        }

        public void HideDecisionMakerView()
        {
            decisionMakerView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            decisionMakerView.RootView = rootView;
        }
    }
}