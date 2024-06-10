#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public static class UtilityIntelligenceEditorUtils
    {
        private static UtilityIntelligenceAsset asset;

        private static UtilityIntelligenceView view;

        private static TabView tabView;

        public static UtilityIntelligenceViewModel ViewModel
        {
            get
            {
                if (asset == null) return null;
                
                return asset.ViewModel as UtilityIntelligenceViewModel;
            }
        }

        public static UtilityIntelligenceModel Model => ViewModel?.Model;

        public static TargetFilterEditorListViewModel TargetFilters => ViewModel.TargetFilters;

        public static BlackboardViewModel Blackboard => ViewModel?.Blackboard;

        public static InputListViewModel Inputs => ViewModel?.Inputs;
        public static ConsiderationEditorListViewModel Considerations => ViewModel?.Considerations;

        public static UtilityIntelligenceAsset Asset => asset;

        public static UtilityIntelligenceView View => view;

        public static TabView TabView => tabView;

        public static void SetView(UtilityIntelligenceView newView)
        {
            if (view == newView)
                return;

            view = newView;

            if (view != null)
                tabView = view.Q<TabView>();
            else
                tabView = null;
        }

        public static void SetAsset(UtilityIntelligenceAsset newAsset)
        {
            if (asset == newAsset)
                return;

            if (asset != null)
            {
                asset.ResetAndSave();
            }

            asset = newAsset;

            if (asset != null)
            {
                Asset.IsEditorOpening = true;
            }
        }
        
        public static void UpdateView(UtilityIntelligenceAsset asset, UtilityIntelligenceViewModel viewModel)
        {
            asset.ViewModel = viewModel;
            SetAsset(asset);
            view.UpdateView(viewModel);

            viewModel.MakeDecision();
        }

        public static void Reset()
        {
            SetView(null);
            SetAsset(null);
        }
    }
}