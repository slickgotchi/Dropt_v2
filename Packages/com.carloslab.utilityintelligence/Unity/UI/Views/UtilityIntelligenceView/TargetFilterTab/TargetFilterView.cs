namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterView : NameView<TargetFilterItemViewModel>
    {
        private ObjectEditorView<TargetFilterItemViewModel> editorView;

        public TargetFilterView() : base(UIBuilderResourcePaths.NameView)
        {
        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            
            editorView = new();
            Container.Add(editorView);
        }

        protected override void OnUpdateView(TargetFilterItemViewModel viewModel)
        {
            editorView.UpdateView(viewModel);
        }

        protected override void OnRefreshView(TargetFilterItemViewModel viewModel)
        {
            base.OnRefreshView(viewModel);
            TitleLabel.text = viewModel.TypeName;
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            base.OnResetView();
            editorView.RootView = rootView;
        }
    }
}