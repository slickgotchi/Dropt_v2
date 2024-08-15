using System.Collections.Generic;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionListViewIntelligenceTab : MainTypeTargetScoreListView<ActionListViewModelIntelligenceTab, ActionItemViewModelIntelligenceTab, DecisionSubViewIntelligenceTab>
    {
        public ActionListViewIntelligenceTab()
        {
            LoadStyleSheet(UIBuilderResourcePaths.StatusListView);
        }

        protected override void RegisterColumns(MultiColumnListView listView)
        {
            RegisterNameColumn(listView);
            RegisterTargetColumn(listView);
            RegisterControlsColumn(listView);
        }

        protected override void OnInitSubView(DecisionSubViewIntelligenceTab subView)
        {
            SubView.ActionView.Hidden += () => ClearSelection();
        }

        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new ActionNameItemViewIntelligenceTab();
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
            element.dataSource = ViewModel.DecisionContextViewModel;
        }

        #endregion

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowActionEditorView(SelectedItem);
            else
                SubView.HideActionEditorView();
        }
        
        protected override VisualElement MakeControlsCell()
        {
            return new VisualElement();
        }
    }
}