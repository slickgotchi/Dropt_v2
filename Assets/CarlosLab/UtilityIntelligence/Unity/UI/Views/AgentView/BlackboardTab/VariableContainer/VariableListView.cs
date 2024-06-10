#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableListView : NameValueListView<BlackboardViewModel, VariableViewModel>
    {
        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new VariableNameItemView(this);
        }

        #endregion
    }
}