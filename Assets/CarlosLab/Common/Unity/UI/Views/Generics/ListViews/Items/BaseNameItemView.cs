#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class BaseNameItemView<TItemViewModel> : BaseItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel
    {
        private readonly Label nameLabel;
        private TextField renameField;

        public BaseNameItemView(bool enableRename, IListViewWithItem<TItemViewModel> listView) : base(listView, null)
        {
            nameLabel = new();
            nameLabel.style.flexGrow = 1.0f;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            Add(nameLabel);

            AddRemoveFunction();
            if (enableRename) AddRenameFunction();
        }

        protected Label NameLabel => nameLabel;

        #region Delete Function

        protected void AddRemoveFunction()
        {
            nameLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Remove", menuAction => { RemoveFromList(); });
            }));
        }

        #endregion

        #region Rename Function

        protected void AddRenameFunction()
        {
            CreateRenameField();

            nameLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2) EnableRenameMode();
            });

            nameLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.InsertAction(0, "Rename", menuAction => { EnableRenameMode(); });
            }));
        }

        private void CreateRenameField()
        {
            renameField = new TextField();
            renameField.style.flexGrow = 1;
            renameField.SetDisplay(false);

            renameField.RegisterCallback<FocusOutEvent>(evt =>
            {
                DisableRenameMode();
                string newName = renameField.text;
                
                if(!string.IsNullOrEmpty(newName))
                    RenameItem(newName);
            });

            Add(renameField);
        }

        protected virtual void RenameItem(string newName)
        {
        }

        private void EnableRenameMode()
        {
            nameLabel.SetDisplay(false);

            renameField.SetValueWithoutNotify(NameLabel.text);
            renameField.SetDisplay(true);

            renameField.focusable = true;
            renameField.schedule.Execute(() => renameField.Focus()).StartingIn(100);
        }

        private void DisableRenameMode()
        {
            nameLabel.SetDisplay(true);

            renameField.SetDisplay(false);
        }

        #endregion
    }
}