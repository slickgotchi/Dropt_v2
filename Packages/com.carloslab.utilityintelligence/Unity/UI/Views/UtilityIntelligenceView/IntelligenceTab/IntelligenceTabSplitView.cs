#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using CarlosLab.UtilityIntelligence.Editor;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class IntelligenceTabSplitView : SplitView<IntelligenceTabMainView, IntelligenceTabSubView, UtilityIntelligenceView>
    {
        public IntelligenceTabSplitView()
        {
            fixedPaneInitialDimension = FrameworkEditorPrefs.IntelligenceSplitView_FixedPaneWidth;
        }

        protected override void FixedPane_OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!RootView.IsIntelligenceTabActive) return;
            FrameworkEditorPrefs.IntelligenceSplitView_FixedPaneWidth = evt.newRect.width;
        }
    }
}