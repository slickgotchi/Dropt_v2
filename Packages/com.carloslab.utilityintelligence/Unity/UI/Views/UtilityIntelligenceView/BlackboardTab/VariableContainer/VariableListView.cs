#region

using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableListView : NameValueListView<BlackboardViewModel, VariableViewModel>
    {
        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new VariableNameItemView();
        }

        protected override VisualElement MakeValueCell()
        {
            return new VariableValueItemView();
        }

        #endregion
    }
}