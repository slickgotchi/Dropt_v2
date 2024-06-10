#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public interface IMainView<TSubView> : IView
        where TSubView : VisualElement, IView
    {
        // void Init(TSubView subView);
        TSubView SubView { get; }

        void InitSubView(TSubView subView);
    }
}