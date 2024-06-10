#region

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerNameItemView : WinnerStatusNameItemView<DecisionMakerViewModel>
    {
        public DecisionMakerNameItemView(DecisionMakerListView listView) : base(true, listView)
        {
            AddRenameFunction();
        }

        protected override void RenameItem(string newName)
        {
            ListView.TryRenameItem(ViewModel, newName);
        }
    }
}