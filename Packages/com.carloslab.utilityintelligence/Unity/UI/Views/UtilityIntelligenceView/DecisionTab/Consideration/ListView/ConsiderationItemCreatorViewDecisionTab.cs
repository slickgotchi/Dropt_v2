#region

using System;
using CarlosLab.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationItemCreatorViewDecisionTab
        : ItemCreatorView<ConsiderationListViewModelDecisionTab, ConsiderationItemViewModelDecisionTab>
    {
        private static readonly ConsiderationItemViewModel CreateNewConsideration = new();
        private PopupField<ConsiderationItemViewModel> considerationField;


        protected override string CreateButtonText { get; } = "Add";

        public ConsiderationItemCreatorViewDecisionTab() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {

        }

        #region View Functions

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            
            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");
            considerationField = CreateConsiderationField();
            typeFieldContainer.Add(considerationField);

            FormatConsiderationField();
            HandleConsiderationFieldValueChanged();
        }

        protected override void OnRefreshView(ConsiderationListViewModelDecisionTab viewModel)
        {
            UpdateConsiderationFieldChoices(viewModel.ConsiderationListViewModel);
        }

        protected override void CreateNewItem()
        {
            ConsiderationItemViewModel consideration = considerationField.value;
            if (consideration != null) ViewModel.TryCreateItem(consideration.Model, out _);
        }

        #endregion

        #region Helper Functions

        private void ValidateButton()
        {
            ConsiderationItemViewModel consideration = considerationField.value;
            string name = consideration?.Name;
            bool isValidated = ValidateName(name);
            createButton.SetEnabled(isValidated);
        }

        private bool ValidateName(string name)
        {
            if (IsRuntime 
                || string.IsNullOrEmpty(name) 
                || ViewModel == null) return false;

            bool nameExists = ViewModel.Contains(name);
            return !nameExists;
        }
        
        public void MakeDecision()
        {
            var intelligenceViewModel = ViewModel.RootViewModel;
            intelligenceViewModel.MakeDecision();
        }

        #endregion

        #region Init ConsiderationField

        private PopupField<ConsiderationItemViewModel> CreateConsiderationField()
        {
            PopupField<ConsiderationItemViewModel> considerationField = new();
            considerationField.label = "Name";
            return considerationField;
        }

        private void SortConsiderationFieldChoices()
        {
            considerationField.choices.Sort((a, b) =>
            {
                if (a == null) return -1;

                if (b == null) return 1;
                return string.CompareOrdinal(a.Name, b.Name);
            });
        }

        private void FormatConsiderationField()
        {
            considerationField.formatListItemCallback = FormatItem;
            considerationField.formatSelectedValueCallback = FormatItem;

            string FormatItem(ConsiderationItemViewModel consideration)
            {
                if (consideration == null)
                    return "None";

                if (consideration == CreateNewConsideration)
                    return "CREATE NEW";

                return consideration.Name;
            }
        }

        private void HandleConsiderationFieldValueChanged()
        {
            considerationField.RegisterValueChangedCallback(evt =>
            {
                ConsiderationItemViewModel newConsideration = evt.newValue;

                if (newConsideration == CreateNewConsideration)
                {
                    RootView.SelectConsiderationTab();
                    considerationField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateButton();
            });
        }

        #endregion

        #region Update ConsiderationField

        private void UpdateConsiderationFieldChoices(ConsiderationListViewModel editorListViewModel)
        {
            if (editorListViewModel == null) return;

            considerationField.SetValueWithoutNotify(null);
            considerationField.choices.Clear();

            var considerations = editorListViewModel.Items;
            for (int index = 0; index < considerations.Count; index++)
            {
                ConsiderationItemViewModel consideration = considerations[index];
                considerationField.choices.Add(consideration);
            }

            // if (considerationField.choices.Count > 0 && considerationField.value == null)
            //     considerationField.value = considerationField.choices[0];

            considerationField.choices.Add(CreateNewConsideration);
        }

        #endregion
        
        #region ViewModel Events
        
        protected override void OnRegisterViewModelEvents(ConsiderationListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded += ViewModel_OnItemAdded;
            viewModel.ItemRemoved += ViewModel_OnItemRemoved;
            
            viewModel.ConsiderationAdded += ViewModel_OnConsiderationAdded;
            viewModel.ConsiderationRemoved += ViewModel_OnConsiderationRemoved;
            viewModel.ConsiderationNameChanged += ViewModel_OnConsiderationNameChanged;
        }
        
        protected override void OnUnregisterViewModelEvents(ConsiderationListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded -= ViewModel_OnItemAdded;
            viewModel.ItemRemoved -= ViewModel_OnItemRemoved;
            
            viewModel.ConsiderationAdded -= ViewModel_OnConsiderationAdded;
            viewModel.ConsiderationRemoved -= ViewModel_OnConsiderationRemoved;
            viewModel.ConsiderationNameChanged -= ViewModel_OnConsiderationNameChanged;
        }
        
        private void ViewModel_OnConsiderationNameChanged(ConsiderationItemViewModel item, string oldName, string newName)
        {
            if (considerationField.value == item)
                considerationField.SetValueWithoutNotify(item);
        }

        private void ViewModel_OnConsiderationAdded(ConsiderationItemViewModel consideration)
        {
            var choices = considerationField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, consideration);
        }

        private void ViewModel_OnConsiderationRemoved(ConsiderationItemViewModel consideration)
        {
            var considerations = considerationField.choices;
            if (considerations.Remove(consideration))
            {
                if (considerationField.value?.Name == consideration.Name)
                    considerationField.value = null;
            }
        }

        private void ViewModel_OnItemAdded(ConsiderationItemViewModelDecisionTab item)
        {
            ValidateButton();
            MakeDecision();
        }

        private void ViewModel_OnItemRemoved(ConsiderationItemViewModelDecisionTab item)
        {
            ValidateButton();

            MakeDecision();
        }

        #endregion
    }
}