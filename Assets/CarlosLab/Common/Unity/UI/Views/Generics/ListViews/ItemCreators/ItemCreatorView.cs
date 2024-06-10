#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class
        ItemCreatorView<TListViewModel, TItemViewModel> : BaseView<TListViewModel>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : IItemViewModel

    {
        protected Button createButton;

        protected virtual string CreateButtonText { get; } = "Create";

        public ItemCreatorView(string visualAssetPath) : base(visualAssetPath)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            createButton = this.Q<Button>("CreateButton");
            createButton.text = CreateButtonText;
            createButton.clicked += OnCreateButtonClicked;
        }

        private void OnCreateButtonClicked()
        {
            CreateNewItem();
        }

        protected abstract void CreateNewItem();
    }
}