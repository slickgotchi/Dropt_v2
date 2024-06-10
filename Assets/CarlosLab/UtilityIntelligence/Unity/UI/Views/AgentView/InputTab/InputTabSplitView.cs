#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class InputTabSplitView : SplitView<InputTabMainView, InputTabSubView>
    {
        protected override void OnHandleAttachToPanel()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}