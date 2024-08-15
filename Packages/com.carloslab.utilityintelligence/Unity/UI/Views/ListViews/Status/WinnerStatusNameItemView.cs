#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class WinnerStatusNameItemView<TItemViewModel> : StatusBasicNameItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel, IStatusViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {
        private const string WinnerClassName = "winner";

        private bool isWinner;

        protected WinnerStatusNameItemView(bool enableStatus, bool enableRename, bool enableRemove) : base(enableStatus, enableRename, enableRemove)
        {
        }

        [CreateProperty]
        public bool IsWinner
        {
            get => isWinner;
            set
            {
                if (isWinner == value)
                    return;

                isWinner = value;
                if (isWinner)
                    EnableStatusClass(WinnerClassName);
                else
                    DisableStatusClass(WinnerClassName);
            }
        }
        
        // protected override void OnUpdateView(TItemViewModel viewModel)
        // {
        //     if (!ViewModel.Asset.IsRuntimeAsset)
        //     {
        //         EnableWinner();
        //     }
        // }

        protected override void OnEnableRuntimeMode()
        {
            base.OnEnableRuntimeMode();
            IsWinner = false;
        }
    }
}