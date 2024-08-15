#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [RequireComponent(typeof(UIDocument))]
    [AddComponentMenu(FrameworkRuntimeConsts.AddRuntimeEditorMenuPath)]
    public class UtilityIntelligenceRuntimeEditor : MonoBehaviour
    {
        [SerializeField]
        private KeyCode hideKey;

        private VisualElement root;
        private UIDocument uiDocument;
        
        private UtilityIntelligenceViewController viewController;

        public UtilityIntelligenceViewController ViewController
        {
            get
            {
                if (viewController == null)
                {
                    viewController = CreateViewController();
                }

                return viewController;
            }
        }

        public bool IsVisible => ViewController.IsVisible;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
        }
        
        private UtilityIntelligenceViewController CreateViewController()
        {
            viewController = new(true);
            UtilityIntelligenceView view = viewController.View;
            view.style.backgroundColor = new Color(200, 200, 200);
            view.SetVisibility(false);
            root.Add(view);

            return viewController;
        }

        public UtilityIntelligenceViewModel CreateViewModel(UtilityIntelligenceAsset asset)
        {
            return ViewController.CreateViewModel(asset);
        }

        private void Update()
        {
            if(UnityEngine.Input.GetKeyDown(hideKey))
                HideView();
        }

        public void ShowView(UtilityIntelligenceViewModel viewModel)
        {
            if (viewController == null) return;

            viewController.UpdateView(viewModel);
            viewController.SetVisibility(true);
        }

        public void HideView()
        {
            if (viewController == null) return;
            
            viewController.CloseEditor();
            viewController.SetVisibility(false);
        }
    }
}