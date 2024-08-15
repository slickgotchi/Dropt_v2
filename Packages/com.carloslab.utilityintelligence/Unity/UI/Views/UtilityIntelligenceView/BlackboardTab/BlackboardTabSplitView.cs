#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class BlackboardTabSplitView : SplitView<BlackboardTabMainView, UtilityIntelligenceViewMember, UtilityIntelligenceView>
    {
        public BlackboardTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}