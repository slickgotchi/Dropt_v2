#region

using System;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class SplitView<TMainView, TSubView> : TwoPaneSplitView, IView
        where TMainView : BaseView, IMainView<TSubView>, new()
        where TSubView : VisualElement, IView, new()
    {
        public SplitView()
        {
            MainView = new TMainView();
            SubView = new TSubView();

            MainView.InitSubView(SubView);

            Add(MainView);
            Add(SubView);

            RegisterCallback<AttachToPanelEvent>(evt => HandleAttachToPanel());
        }

        public TMainView MainView { get; }
        public TSubView SubView { get; }

        public event Action Shown;
        public event Action Hidden;

        public void Show(bool show)
        {
            this.SetDisplay(show);
            if (show)
                RaiseShownEvent();
            else
                RaiseHiddenEvent();
        }

        private void HandleAttachToPanel()
        {
            MainView.style.minWidth = 120;
            // SubView.style.minWidth = 120;

            fixedPaneIndex = 0;
            fixedPaneInitialDimension = 256;

            OnHandleAttachToPanel();
        }

        protected virtual void OnHandleAttachToPanel()
        {
        }

        public void RaiseShownEvent()
        {
            Shown?.Invoke();
            OnShown();
        }

        public void RaiseHiddenEvent()
        {
            Hidden?.Invoke();
            OnHidden();
        }

        protected virtual void OnShown()
        {
        }

        protected virtual void OnHidden()
        {
        }
    }
}