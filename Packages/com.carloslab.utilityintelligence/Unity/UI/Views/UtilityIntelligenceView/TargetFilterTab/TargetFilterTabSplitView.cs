using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class TargetFilterTabSplitView : SplitView<TargetFilterTabMainView, TargetFilterTabSubView>
    {
        public TargetFilterTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}