
namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationTabSplitView : SplitView<InputNormalizationTabMainView, InputNormalizationTabSubView>
    {
        public InputNormalizationTabSplitView()
        {
            MainView.style.minWidth = 256;
            fixedPaneInitialDimension = 400;
        }
    }
}