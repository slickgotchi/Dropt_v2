using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class MainView<TViewModel, TSubView> : MainView<TViewModel, TSubView, UtilityIntelligenceView>
        where TViewModel : class, IViewModel
        where TSubView : BaseView, IRootViewMember
    {
        public MainView(string visualAssetPath) : base(visualAssetPath)
        {
        }
    }
}