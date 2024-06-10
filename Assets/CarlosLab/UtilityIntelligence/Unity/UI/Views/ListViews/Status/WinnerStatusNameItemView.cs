#region

using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class WinnerStatusNameItemView<TItemViewModel> : StatusBasicNameItemView<TItemViewModel>
        where TItemViewModel : BaseItemViewModel, INameViewModel, IStatusViewModel, IWinnerViewModel

    {
        private const string WinnerClassName = "winner";

        private bool isWinner;

        private readonly DataBinding winnerBinding = new()
        {
            bindingMode = BindingMode.ToTarget,
            updateTrigger = BindingUpdateTrigger.OnSourceChanged,
            dataSourcePath = PropertyPath.FromName(nameof(IWinnerViewModel.IsWinner))
        };

        protected WinnerStatusNameItemView(bool enableStatus,
            IListViewWithItem<TItemViewModel> listView) : base(enableStatus, listView)
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
                    EnableStateClass(WinnerClassName);
                else
                    DisableStateClass(WinnerClassName);
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
            DisableWinner();
        }

        protected override void OnEnableEditMode()
        {
            base.OnEnableEditMode();
            
            EnableWinner();
        }

        private void DisableWinner()
        {
            SetBinding(nameof(IsWinner), null);
            IsWinner = false;
            
            // Debug.Log($"DisableWinner IsRuntimeAsset: {ViewModel.Asset.IsRuntimeAsset} IsWinner: {isWinner}");
        }

        private void EnableWinner()
        {
            SetBinding(nameof(IsWinner), winnerBinding);
            // Debug.Log($"EnableWinner IsRuntimeAsset: {ViewModel.Asset.IsRuntimeAsset} IsWinner: {isWinner}");
        }
    }
}