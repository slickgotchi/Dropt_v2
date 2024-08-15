﻿using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationViewIntelligenceTab : NameView<ConsiderationItemViewModelIntelligenceTab>
    {
        private Button editButton;
        private InputNormalizationViewIntelligenceTab inputView;

        private ResponseCurveViewIntelligenceTab responseCurveView;

        public ConsiderationViewIntelligenceTab() : base(UIBuilderResourcePaths.NameView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            
            TitleLabel.text = "Consideration";
            inputView = new();
            inputView.style.marginTop = 5.0f;
            Container.Add(inputView);

            responseCurveView = new();
            Container.Add(responseCurveView);

            editButton = new();
            editButton.text = "Edit";
            editButton.style.width = 100;
            editButton.style.marginTop = 20;
            editButton.style.marginLeft = 30;

            editButton.style.alignSelf = Align.Center;
            editButton.clicked += SwitchToEditView;
            Container.Add(editButton);
        }


        private void SwitchToEditView()
        {
            RootView.SelectConsiderationTab(ViewModel.ConsiderationViewModel.Index);
        }

        protected override void OnUpdateView(ConsiderationItemViewModelIntelligenceTab viewModel)
        {
            inputView.UpdateView(viewModel);
            responseCurveView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputView.RootView = rootView;
            responseCurveView.RootView = rootView;
        }
    }
}