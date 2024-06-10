#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class NameView<TViewModel>
        : BaseView<TViewModel>
        where TViewModel : ViewModel, INameViewModel
    {
        protected ScrollView container;
        private TextField nameField;

        protected Label titleLabel;

        protected NameView(string visualAssetPath) : base(visualAssetPath)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            titleLabel = this.Q<Label>("TitleLabel");
            container = this.Q<ScrollView>("Container");
            nameField = this.Q<TextField>("NameField");
            
            nameField.textEdition.placeholder = "Name...";
            nameField.SetEnabled(false);
        }

        public sealed override bool UpdateView(TViewModel viewModel)
        {
            bool result = base.UpdateView(viewModel);

            if (result)
                nameField.SetDataBinding(nameof(TextField.value), nameof(INameViewModel.Name), BindingMode.ToTarget);

            return result;
        }
    }
}