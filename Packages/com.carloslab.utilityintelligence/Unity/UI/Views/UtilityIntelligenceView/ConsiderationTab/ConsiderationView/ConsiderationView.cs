#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationView : NameView<ConsiderationItemViewModel>
    {
        private Toggle hasNoTargetToggle;
        private Toggle enableCachePerTargetToggle;

        private InputNormalizationViewConsiderationTab inputNormalizationView;
        private ResponseCurveView responseCurveView;

        public ConsiderationView() : base(UIBuilderResourcePaths.ConsiderationView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            
            InitHasNoTargetToggle();

            InitEnableCachePerTargetToggle();

            inputNormalizationView = this.Q<InputNormalizationViewConsiderationTab>();

            responseCurveView = this.Q<ResponseCurveView>();
        }

        private void InitHasNoTargetToggle()
        {
            hasNoTargetToggle = this.Q<Toggle>("HasNoTargetToggle");

            hasNoTargetToggle.RegisterValueChangedCallback(evt =>
            {
                bool hasNoTarget = evt.newValue;
                ViewModel.HasNoTarget = hasNoTarget;
                enableCachePerTargetToggle.SetDisplay(!hasNoTarget);
            });
        }
        
        private void InitEnableCachePerTargetToggle()
        {
            enableCachePerTargetToggle = this.Q<Toggle>("EnableCachePerTargetToggle");
            enableCachePerTargetToggle.RegisterValueChangedCallback(evt =>
            {
                ViewModel.EnableCachePerTarget = evt.newValue;
            });
        }

        protected override void OnUpdateView(ConsiderationItemViewModel viewModel)
        {
            inputNormalizationView.UpdateView(viewModel);
            responseCurveView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(ConsiderationItemViewModel viewModel)
        {
            base.OnRefreshView(viewModel);
            
            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(ConsiderationItemViewModel.HasNoTarget), 
                BindingMode.ToTarget);
            
            enableCachePerTargetToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(ConsiderationItemViewModel.EnableCachePerTarget), 
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            hasNoTargetToggle.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputNormalizationView.RootView = rootView;
            responseCurveView.RootView = rootView;
        }
    }
}