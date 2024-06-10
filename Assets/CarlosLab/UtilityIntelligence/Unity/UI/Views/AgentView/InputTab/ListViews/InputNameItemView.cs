#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNameItemView : BasicNameItemView<InputItemViewModel>
    {
        public InputNameItemView(InputListView listView) : base(true, listView)
        {
        }

        protected override void RenameItem(string newName)
        {
            ListView.TryRenameItem(ViewModel, newName);
        }
    }
}