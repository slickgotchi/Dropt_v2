﻿#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class StatusBasicNameItemView<TItemViewModel> : StatusBaseNameItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel, IStatusViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        protected StatusBasicNameItemView(bool enableStatus, bool enableRename, bool enableRemove) : base( enableStatus, enableRename, enableRemove)
        {
        }

        protected override void OnRefreshView(TItemViewModel viewModel)
        {
            NameLabel.SetDataBinding(nameof(Label.text), nameof(INameViewModel.Name), BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            NameLabel.ClearBindings();
        }
    }
}