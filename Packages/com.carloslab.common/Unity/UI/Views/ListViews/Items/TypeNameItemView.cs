using UnityEngine.UIElements;

namespace CarlosLab.Common.UI
{
    public abstract class TypeNameItemView<TItemViewModel, TRootView>
        : BaseNameItemView<TItemViewModel, TRootView>
        where TItemViewModel : class, IRootViewModelMember, ITypeNameViewModel, IItemViewModel
        where TRootView: BaseView, IRootView
    {
        protected TypeNameItemView() : base( false)
        {
        }

        protected override void OnRefreshView(TItemViewModel viewModel)
        {
            NameLabel.text = viewModel.TypeName;
        }
    }
}