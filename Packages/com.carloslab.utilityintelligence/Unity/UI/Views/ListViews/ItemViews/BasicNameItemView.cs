﻿using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class BasicNameItemView<TItemViewModel> : BasicNameItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IRootViewModelMember<UtilityIntelligenceViewModel>, IItemViewModel, INameViewModel
    {
        public BasicNameItemView(bool enableRename, bool enableRemove = true) : base(enableRename, enableRemove)
        {
        }
    }
}