#region

using System;
using System.Runtime.CompilerServices;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

namespace CarlosLab.Common.UI
{
    public abstract class BaseView : VisualElement, IView
    {
        public string Id { get; }

        private VisualTreeAsset visualAsset;

        public BaseView(string visualAssetPath)
        {
            Id = Guid.NewGuid().ToString();

            LoadVisualAsset(visualAssetPath);

            RegisterCallback<AttachToPanelEvent>(HandleAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(HandleDetachFromPanel);
        }

        public event Action Shown;
        public event Action Hidden;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        protected void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        #region Visual/Style Functions

        protected void LoadVisualAsset(string visualAssetPath)
        {
            if (string.IsNullOrEmpty(visualAssetPath))
                return;

            visualAsset = Resources.Load<VisualTreeAsset>(visualAssetPath);
            if (visualAsset == null) return;

            visualAsset.CloneTree(this);

            OnVisualAssetLoaded();
        }

        protected virtual void OnVisualAssetLoaded()
        {
            
        }

        protected void LoadStyleSheet(string styleSheetPath)
        {
            if (string.IsNullOrEmpty(styleSheetPath))
                return;

            StyleSheet styleSheet = Resources.Load<StyleSheet>(styleSheetPath);
            if (styleSheet == null) return;

            styleSheets.Add(styleSheet);
        }

        #endregion

        #region Show/Hide View Functions

        public void Show(bool show)
        {
            this.SetDisplay(show);
            if (show)
                HandleShownEvent();
            else
                HandleHiddenEvent();
        }

        public void HandleShownEvent()
        {
            Shown?.Invoke();
            OnShown();
        }

        public void HandleHiddenEvent()
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

        #endregion

        #region Attach/Detach Panel Functions

        private void HandleAttachToPanel(AttachToPanelEvent evt)
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
            OnAttachToPanel(evt);
        }

        private void HandleDetachFromPanel(DetachFromPanelEvent evt)
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
            OnDetachFromPanel(evt);
        }
        
#if UNITY_EDITOR
        protected virtual void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {

        }
#endif

        protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
        {
        }

        protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
        }

        #endregion
    }
}