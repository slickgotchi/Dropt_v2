using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class UtilityIntelligenceViewController
    {
        private UtilityIntelligenceView view;

        public UtilityIntelligenceView View => view;

        public bool IsRuntimeUI => view?.IsRuntimeUI ?? false;

        public bool IsVisible => view?.IsVisible() ?? false;

        public UtilityIntelligenceViewModel ViewModel => view.ViewModel;

        public UtilityIntelligenceAsset Asset => ViewModel?.Asset;

        public UtilityIntelligenceViewController(bool isRuntimeUI)
        {
            view = new(isRuntimeUI);
        }
        
        
        //Cause Issues with different ViewModels but same assets
        public void UpdateView(UtilityIntelligenceViewModel newViewModel)
        {
            var oldViewModel = ViewModel;
            if (oldViewModel == newViewModel) return;

            bool isDifferentAsset = oldViewModel?.Asset != newViewModel.Asset;
            
            CloseEditor(oldViewModel, isDifferentAsset);
                
            OpenEditor(newViewModel, isDifferentAsset);
        }
        
        public UtilityIntelligenceViewModel CreateViewModel(UtilityIntelligenceAsset asset)
        {
            UtilityIntelligenceViewModel viewModel = new();
            viewModel.Init(asset);
            return viewModel;
        }

        public void SetVisibility(bool visibility)
        {
            view.SetVisibility(visibility);
        }

        public void CloseEditor()
        {
            CloseEditor(ViewModel, true);
        }

        private void CloseEditor(UtilityIntelligenceViewModel viewModel, bool closeAsset)
        {
            if (viewModel == null) return;
            
            view.ClearView();
            view.ClearViewModel();
            // view.InitView();

            if (closeAsset)
            {
                viewModel.Asset.CloseEditor(view.IsRuntimeUI);
            }
            
            viewModel.ClearModel();
        }

        private void OpenEditor(UtilityIntelligenceViewModel viewModel, bool openAsset)
        {
            if (viewModel == null) return;

            view.InitView();
            view.UpdateView(viewModel);

            if(openAsset)
                viewModel.Asset.OpenEditor(view.IsRuntimeUI);

            viewModel.MakeDecision();
        }
    }
}