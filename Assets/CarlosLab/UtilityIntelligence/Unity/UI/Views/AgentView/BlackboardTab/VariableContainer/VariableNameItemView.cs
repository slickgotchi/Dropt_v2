#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableNameItemView : BasicNameItemView<VariableViewModel>
    {
        public VariableNameItemView(IListViewWithItem<VariableViewModel> listView) : base(true, listView)
        {
        }

        protected override void RenameItem(string newName)
        {
            ListView.TryRenameItem(ViewModel, newName);
        }
    }
}