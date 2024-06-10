#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationEditorView : NameView<ConsiderationEditorViewModel>
    {
        private Toggle hasNoTargetToggle;
        private InputSelectionView inputView;
        private ResponseCurveEditorView responseCurveEditorView;

        public ConsiderationEditorView() : base(UIBuilderResourcePaths.ConsiderationEditorView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            
            hasNoTargetToggle = this.Q<Toggle>("HasNoTargetToggle");
            
            hasNoTargetToggle.RegisterValueChangedCallback(evt =>
            {
                bool hasNoTarget = evt.newValue;
                ViewModel.HasNoTarget = hasNoTarget;
            });
            
            inputView = this.Q<InputSelectionView>();

            responseCurveEditorView = this.Q<ResponseCurveEditorView>();
            
        }

        protected override void OnUpdateView(ConsiderationEditorViewModel viewModel)
        {
            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(ConsiderationEditorViewModel.HasNoTarget), 
                BindingMode.ToTarget);
            
            inputView.UpdateView(viewModel);
            responseCurveEditorView.UpdateView(viewModel.RuntimeViewModel);
        }
    }
}