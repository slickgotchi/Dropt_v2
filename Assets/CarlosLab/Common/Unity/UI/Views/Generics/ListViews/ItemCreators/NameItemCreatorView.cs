#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class
        NameItemCreatorView<TListViewModel, TItemViewModel> : ItemCreatorView<TListViewModel, TItemViewModel>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, INameListViewModel
        where TItemViewModel : class, IItemViewModel, INameViewModel
    {
        private TextField nameField;
        
        public NameItemCreatorView(string visualAssetPath = UIBuilderResourcePaths.NameItemCreatorView) : base(
            visualAssetPath)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            nameField = this.Q<TextField>("NameField");
            nameField.label = "Name";
            nameField.RegisterCallback<ChangeEvent<string>>(evt => { ValidateButton(); });
            
            ValidateButton();
        }

        protected void ValidateButton()
        {
            bool isValidated = ValidateName(nameField.text);
            createButton.SetEnabled(isValidated);
        }

        private bool ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if(ViewModel == null) return false;

            bool nameExists = ViewModel.Contains(name);
            return !nameExists;
        }

        protected override void CreateNewItem()
        {
            ViewModel.CreateItem(null, nameField.text);
        }

        protected override void OnRegisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded += OnItemAdded;
            viewModel.ItemRemoved += OnItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnItemAdded;
            viewModel.ItemRemoved -= OnItemRemoved;
        }

        protected virtual void OnItemAdded(TItemViewModel item)
        {
            ValidateButton();
        }

        protected virtual void OnItemRemoved(TItemViewModel item)
        {
            ValidateButton();
        }
    }
}