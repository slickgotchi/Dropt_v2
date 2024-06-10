#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ObjectNameEditorView<TViewModel> : BaseView<TViewModel>
        where TViewModel : ViewModel, ITypeNameViewModel
    {
        private readonly ObjectEditorView<TViewModel> editorView;
        private readonly Label nameLabel;

        public ObjectNameEditorView() : base(null)
        {
            nameLabel = UIElementsFactory.CreateTitleLabel();
            Add(nameLabel);
            editorView = new ObjectEditorView<TViewModel>();
            Add(editorView);
        }

        protected override void OnUpdateView(TViewModel viewModel)
        {
            nameLabel.text = viewModel.TypeName;
            editorView.UpdateView(viewModel);
        }
    }
}