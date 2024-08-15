#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class
        BaseItemCreatorView<TListViewModel, TItemViewModel, TRootView> : RootViewMember<TListViewModel, TRootView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember
        where TRootView: BaseView, IRootView

    {
        protected Button createButton;

        protected virtual string CreateButtonText { get; } = "Create";

        public BaseItemCreatorView(string visualAssetPath) : base(visualAssetPath)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();

            createButton = this.Q<Button>("CreateButton");
            createButton.text = CreateButtonText;
            createButton.clicked += OnCreateButtonClicked;
            createButton.SetEnabled(false);
        }

        private void OnCreateButtonClicked()
        {
            CreateNewItem();
        }

        protected abstract void CreateNewItem();
    }
}