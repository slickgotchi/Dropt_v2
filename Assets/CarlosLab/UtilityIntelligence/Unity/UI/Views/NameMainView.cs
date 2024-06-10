#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class NameMainView<TViewModel, TSubView>
        : NameView<TViewModel>, IMainView<TSubView>
        where TViewModel : ViewModel, INameViewModel
        where TSubView : VisualElement, IView
    {
        protected NameMainView(string visualAssetPath) : base(visualAssetPath)
        {
            // this.SetDisplay(false);
        }

        public TSubView SubView { get; private set; }

        public void InitSubView(TSubView subView)
        {
            SubView = subView;
            OnInitSubView(subView);
        }

        protected virtual void OnInitSubView(TSubView subView)
        {
        }
    }
}