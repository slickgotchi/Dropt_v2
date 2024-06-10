#region

using System;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public class
        NameTypeItemCreatorView<TListViewModel, TItemViewModel> : TypeItemCreatorView<TListViewModel, TItemViewModel>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, INameListViewModel
        where TItemViewModel : class, IItemViewModel, INameViewModel
    {
        private TextField nameField;

        public NameTypeItemCreatorView() : base( UIBuilderResourcePaths.NameTypeItemCreatorView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            nameField = this.Q<TextField>("NameField");
            nameField.RegisterCallback<ChangeEvent<string>>(evt => { ValidateButton(); });
            
            ValidateButton();
        }

        private void ValidateButton()
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

        protected override void OnRegisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded += OnItemAdded;
            viewModel.ItemRemoved += OnItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(TListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnItemAdded;
        }

        private void OnItemAdded(TItemViewModel item)
        {
            ValidateButton();
        }

        private void OnItemRemoved(TItemViewModel item)
        {
            ValidateButton();
        }

        protected override void CreateNewItem()
        {
            Type type = TypeField.value;
            string name = nameField.text;
            if (type != null) ViewModel.CreateItem(type, name);
        }
    }
}