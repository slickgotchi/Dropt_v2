#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationNameItemView : StatusBasicNameItemView<ConsiderationViewModel>
    {
        public ConsiderationNameItemView(ConsiderationListView listView) : base(true, listView)
        {
        }
    }
}