#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionSplitView : SplitView<DecisionMainView, DecisionSubView>
    {
        public DecisionSplitView()
        {
            MainView.style.minWidth = 256;
            // fixedPaneInitialDimension = 400;
        }
    }
}