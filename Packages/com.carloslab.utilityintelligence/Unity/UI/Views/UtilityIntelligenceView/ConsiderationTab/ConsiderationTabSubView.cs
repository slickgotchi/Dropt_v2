#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationTabSubView : UtilityIntelligenceViewMember
    {
        private readonly ConsiderationView considerationView;

        public ConsiderationTabSubView()
        {
            considerationView = new ConsiderationView();
            considerationView.Show(false);
            Add(considerationView);
        }

        public void ShowEditorView(ConsiderationItemViewModel viewModel)
        {
            considerationView.Show(true);
            considerationView.UpdateView(viewModel);
        }

        public void HideEditorView()
        {
            considerationView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            considerationView.RootView = rootView;
        }
    }
}