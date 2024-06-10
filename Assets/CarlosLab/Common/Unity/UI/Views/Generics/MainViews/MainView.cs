#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public class MainView<TViewModel, TSubView> : BaseView<TViewModel>, IMainView<TSubView>
        where TViewModel : class, IViewModel
        where TSubView : VisualElement, IView
    {
        public MainView(string visualAssetPath) : base(visualAssetPath)
        {
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