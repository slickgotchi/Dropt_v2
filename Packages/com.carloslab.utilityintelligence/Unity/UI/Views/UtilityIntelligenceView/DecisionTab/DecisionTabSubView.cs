using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionTabSubView : UtilityIntelligenceViewMember
    {
        private readonly DecisionSplitView decisionView;

        public DecisionTabSubView()
        {
            decisionView = new DecisionSplitView();
            decisionView.Show(false);
            Add(decisionView);
        }
        
        public void ShowDecisionView(DecisionItemViewModel viewModel)
        {
            decisionView.Show(true);
            decisionView.MainView.UpdateView(viewModel);
        }

        public void HideDecisionView()
        {
            decisionView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            decisionView.RootView = rootView;
        }
    }
}