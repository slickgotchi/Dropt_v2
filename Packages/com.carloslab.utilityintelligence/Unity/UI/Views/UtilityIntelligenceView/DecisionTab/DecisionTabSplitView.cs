using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionTabSplitView : SplitView<DecisionTabMainView, DecisionTabSubView>
    {
        public DecisionTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}