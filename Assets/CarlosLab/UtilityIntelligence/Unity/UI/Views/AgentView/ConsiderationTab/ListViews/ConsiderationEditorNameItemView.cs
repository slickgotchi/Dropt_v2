#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationEditorNameItemView : BasicNameItemView<ConsiderationEditorViewModel>
    {
        public ConsiderationEditorNameItemView(ConsiderationEditorListView listView) : base(true, listView)
        {
        }

        protected override void RenameItem(string newName)
        {
            ListView.TryRenameItem(ViewModel, newName);
        }
    }
}