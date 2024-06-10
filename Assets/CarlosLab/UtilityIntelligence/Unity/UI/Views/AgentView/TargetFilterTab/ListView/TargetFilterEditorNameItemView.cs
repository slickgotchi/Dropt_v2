using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterEditorNameItemView : BasicNameItemView<TargetFilterEditorViewModel>
    {
        public TargetFilterEditorNameItemView(TargetFilterEditorListView listView) : base(true, listView)
        {
        }

        protected override void RenameItem(string newName)
        {
            ListView.TryRenameItem(ViewModel, newName);
        }
    }
}