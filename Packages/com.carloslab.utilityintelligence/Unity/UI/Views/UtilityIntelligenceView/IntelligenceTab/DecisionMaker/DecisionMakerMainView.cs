#region

using CarlosLab.UtilityIntelligence.Editor;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMakerMainView : NameMainView<DecisionMakerItemViewModel, DecisionMakerSubView>
    {
        private DecisionContainerViewIntelligenceTab containerView;

        public DecisionMakerMainView() : base(UIBuilderResourcePaths.DecisionMakerMainView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            
            containerView = this.Q<DecisionContainerViewIntelligenceTab>();
        }

        protected override void OnInitSubView(DecisionMakerSubView subView)
        {
            containerView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionMakerItemViewModel viewModel)
        {
            containerView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            containerView.RootView = rootView;
        }
    }
}