#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ResponseCurveView : BaseView<ConsiderationViewModel>
    {
        private readonly InfluenceCurveGraphView influenceCurveView;

        public ResponseCurveView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Response Curve";
            Add(foldout);

            influenceCurveView = new InfluenceCurveGraphView();
            influenceCurveView.style.alignSelf = Align.Center;
            foldout.Add(influenceCurveView);
        }

        protected override void OnUpdateView(ConsiderationViewModel viewModel)
        {
            if (ViewModel.Asset.IsRuntimeAsset)
            {
                influenceCurveView.dataSource = viewModel.ContextViewModel;
                influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.InfluenceCurve),
                    nameof(ConsiderationRuntimeContextViewModel.ResponseCurve), BindingMode.ToTarget);
                influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.Input),
                    nameof(ConsiderationRuntimeContextViewModel.NormalizedInput), BindingMode.ToTarget);
            }
            else
            {
                influenceCurveView.dataSource = viewModel.RuntimeViewModel;
                influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.InfluenceCurve),
                    nameof(ConsiderationRuntimeViewModel.ResponseCurve), BindingMode.ToTarget);
                influenceCurveView.SetDataBinding(nameof(InfluenceCurveGraphView.Input),
                    nameof(ConsiderationRuntimeViewModel.NormalizedInput), BindingMode.ToTarget);
            }
        }
    }
}