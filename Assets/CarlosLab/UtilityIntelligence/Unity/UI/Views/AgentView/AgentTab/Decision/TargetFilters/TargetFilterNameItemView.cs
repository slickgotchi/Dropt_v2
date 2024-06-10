#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterNameItemView : BasicNameItemView<TargetFilterViewModel>
    {
        public TargetFilterNameItemView(TargetFilterListView listView) : base(false, listView)
        {
        }
    }
}