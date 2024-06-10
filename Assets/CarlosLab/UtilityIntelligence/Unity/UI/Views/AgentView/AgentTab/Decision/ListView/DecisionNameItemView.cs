#region

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionNameItemView : WinnerStatusNameItemView<DecisionViewModel>
    {
        public DecisionNameItemView(DecisionListView listView) : base(true, listView)
        {
            AddRenameFunction();
        }

        protected override void RenameItem(string newName)
        {
            ListView.TryRenameItem(ViewModel, newName);
        }
    }
}