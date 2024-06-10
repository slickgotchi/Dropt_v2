#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ResponseCurveEditorView : BaseView<ConsiderationRuntimeViewModel>
    {
        private readonly InfluenceCurveField responseCurveField;

        public ResponseCurveEditorView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Response Curve";
            Add(foldout);

            responseCurveField = new InfluenceCurveField();
            responseCurveField.style.alignSelf = Align.Center;
            responseCurveField.InputApplied += OnResponseCurveFieldInputApplied;
            foldout.Add(responseCurveField);
        }

        protected override void OnModelChanged()
        {
            responseCurveField.value = ViewModel.ResponseCurve;
        }

        private void OnResponseCurveFieldInputApplied()
        {
            ViewModel.Record($"ResponseCurveEditorView ResponseCurve Changed: {responseCurveField.value.ToString()}",
                () => { ViewModel.ResponseCurve = responseCurveField.value; });
        }

        protected override void OnUpdateView(ConsiderationRuntimeViewModel viewModel)
        {
            responseCurveField.SetDataBinding(nameof(InfluenceCurveField.value),
                nameof(ConsiderationRuntimeViewModel.ResponseCurve), BindingMode.ToTarget);
            responseCurveField.SetDataBinding(nameof(InfluenceCurveField.Input),
                nameof(ConsiderationRuntimeViewModel.NormalizedInput), BindingMode.ToTarget);
        }
    }
}