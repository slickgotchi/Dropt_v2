#region

using System;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public class
        NameTypeItemCreatorView<TListViewModel, TItemViewModel, TRootView> : BaseTypeItemCreatorView<TListViewModel, TItemViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, INameListViewModel, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, INameViewModel, IRootViewModelMember
        where TRootView: BaseView, IRootView
    {
        private TextField nameField;

        public NameTypeItemCreatorView() : base( UIBuilderResourcePaths.NameTypeItemCreatorView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            nameField = this.Q<TextField>("NameField");
            nameField.RegisterCallback<ChangeEvent<string>>(evt => { ValidateButton(); });
        }

        private void ValidateButton()
        {
            bool isValidated = ValidateName(nameField.text);
            createButton.SetEnabled(isValidated);
        }

        private bool ValidateName(string name)
        {
            if (IsRuntime 
                || string.IsNullOrEmpty(name) 
                || ViewModel == null) return false;

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