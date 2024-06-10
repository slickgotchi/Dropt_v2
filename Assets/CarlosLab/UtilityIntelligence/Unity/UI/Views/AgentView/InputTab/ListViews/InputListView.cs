#region

using System.Collections.Generic;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputListView : NameValueListView<InputListViewModel, InputItemViewModel>, IMainView<InputTabSubView>
    {
        public InputTabSubView SubView { get; private set; }

        public void InitSubView(InputTabSubView subView)
        {
            SubView = subView;
        }

        protected override void OnSelectionChanged(IEnumerable<object> items)
        {
            if (SelectedItem != null)
                SubView.ShowInputEditorView(SelectedItem);
            else
                SubView.HideInputEditorView();
        }

        #region Make Cells

        protected override VisualElement MakeNameCell()
        {
            return new InputNameItemView(this);
        }

        protected override VisualElement MakeValueCell()
        {
            return new InputValueItemView(this);
        }

        protected override void BindValueCell(VisualElement element, int index)
        {
            InputValueItemView itemView = element as InputValueItemView;
            itemView?.UpdateView(ViewModel.Items[index]);
        }

        #endregion
    }
}