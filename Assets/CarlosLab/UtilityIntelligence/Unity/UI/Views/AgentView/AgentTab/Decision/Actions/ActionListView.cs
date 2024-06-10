#region

using System.Collections.Generic;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionListView : MainTypeListView<ActionListViewModel, ActionViewModel, DecisionSubView>
    {
        public ActionListView() : base(false, true)
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }

        protected override void OnInitSubView(DecisionSubView subView)
        {
            SubView.ActionEditorView.Hidden += () => ClearSelection();
        }

        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new ActionNameItemView(this);
        }

        // protected override VisualElement MakeTargetCell()
        // {
        //     return new ActionTargetNameItemView(this);
        // }

        #endregion

        #region Bind Cells

        protected override void BindTargetCell(VisualElement element, int index)
        {
            base.BindTargetCell(element, index);
            element.dataSource = ViewModel.ContextViewModel;
        }

        #endregion

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowActionEditorView(SelectedItem);
            else
                SubView.HideActionEditorView();
        }
    }
}