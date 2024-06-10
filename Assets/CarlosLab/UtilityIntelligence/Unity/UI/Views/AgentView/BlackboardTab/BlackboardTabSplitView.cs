#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class BlackboardTabSplitView : SplitView<BlackboardTabMainView, BasicView>
    {
        protected override void OnHandleAttachToPanel()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}