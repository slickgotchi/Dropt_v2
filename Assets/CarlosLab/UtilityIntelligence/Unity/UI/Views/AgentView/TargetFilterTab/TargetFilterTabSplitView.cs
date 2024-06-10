using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class TargetFilterTabSplitView : SplitView<TargetFilterTabMainView, TargetFilterTabSubView>
    {
        protected override void OnHandleAttachToPanel()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}