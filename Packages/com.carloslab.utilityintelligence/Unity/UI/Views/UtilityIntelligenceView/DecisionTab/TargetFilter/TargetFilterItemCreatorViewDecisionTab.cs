#region

using System;
using System.Collections.Generic;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterItemCreatorViewDecisionTab : ItemCreatorView<TargetFilterListViewModelDecisionTab, TargetFilterItemViewModelDecisionTab>
    {
        private static readonly TargetFilterItemViewModel CreateNewTargetFilter = new();
        private PopupField<TargetFilterItemViewModel> targetFilterField;

        protected override string CreateButtonText { get; } = "Add";

        public TargetFilterItemCreatorViewDecisionTab() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");
            
            targetFilterField = CreateTargetFilterField();
            typeFieldContainer.Add(targetFilterField);

            FormatTargetFilterField();
            HandleTargetFilterFieldValueChanged();
        }

        protected override void OnRefreshView(TargetFilterListViewModelDecisionTab viewModel)
        {
            UpdateTargetFilterFieldChoices(viewModel.TargetFilterListViewModel);
        }

        protected void ValidateButton()
        {
            TargetFilterItemViewModel targetFilter = targetFilterField.value;
            string name = targetFilter?.Name;
            bool isValidated = ValidateName(name);
            createButton.SetEnabled(isValidated);
        }

        protected bool ValidateName(string name)
        {
            if (IsRuntime 
                || string.IsNullOrEmpty(name) 
                || ViewModel == null) return false;

            bool nameExists = ViewModel.Contains(name);
            return !nameExists;
        }


        protected override void CreateNewItem()
        {
            TargetFilterItemViewModel targetFilter = targetFilterField.value;
            if (targetFilter != null) ViewModel.TryCreateItem(targetFilter.Model, out _);
        }

        #region Update TargetFilterField

        private void UpdateTargetFilterFieldChoices(TargetFilterListViewModel targetFiltersViewModel)
        {
            if (targetFiltersViewModel == null) return;

            targetFilterField.SetValueWithoutNotify(null);
            targetFilterField.choices.Clear();

            var targetFilters = targetFiltersViewModel.Items;
            for (int index = 0; index < targetFilters.Count; index++)
            {
                TargetFilterItemViewModel targetFilter = targetFilters[index];
                targetFilterField.choices.Add(targetFilter);
            }

            // if (targetFilterField.choices.Count > 0 && targetFilterField.value == null)
            //     targetFilterField.value = targetFilterField.choices[0];

            targetFilterField.choices.Add(CreateNewTargetFilter);
        }

        #endregion

        #region Init TargetFilterField

        private PopupField<TargetFilterItemViewModel> CreateTargetFilterField()
        {
            PopupField<TargetFilterItemViewModel> targetFilterField = new();
            targetFilterField.label = "Name";
            return targetFilterField;
        }

        private void SortTargetFilterFieldChoices()
        {
            targetFilterField.choices.Sort((a, b) =>
            {
                if (a == null) return -1;

                if (b == null) return 1;
                return string.CompareOrdinal(a.Name, b.Name);
            });
        }

        private void FormatTargetFilterField()
        {
            targetFilterField.formatListItemCallback = FormatItem;
            targetFilterField.formatSelectedValueCallback = FormatItem;

            string FormatItem(TargetFilterItemViewModel targetFilter)
            {
                if (targetFilter == null)
                    return "None";

                if (targetFilter == CreateNewTargetFilter)
                    return "CREATE NEW";

                return targetFilter.Name;
            }
        }

        private void HandleTargetFilterFieldValueChanged()
        {
            targetFilterField.RegisterValueChangedCallback(evt =>
            {
                TargetFilterItemViewModel newTargetFilter = evt.newValue;

                if (newTargetFilter == CreateNewTargetFilter)
                {
                    RootView.SelectTargetFilterTab();
                    targetFilterField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateButton();
            });
        }

        #endregion

        #region ViewModel Events

        protected override void OnRegisterViewModelEvents(TargetFilterListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded += ViewModel_OnItemAdded;
            viewModel.ItemRemoved += ViewModel_OnItemRemoved;
            
            viewModel.TargetFilterAdded += ViewModel_OnTargetFilterAdded;
            viewModel.TargetFilterRemoved += ViewModel_OnTargetFilterRemoved;
            viewModel.TargetFilterNameChanged += ViewModel_OnTargetFilterNameChanged;
        }

        protected override void OnUnregisterViewModelEvents(TargetFilterListViewModelDecisionTab viewModel)
        {
            viewModel.ItemAdded -= ViewModel_OnItemAdded;
            viewModel.ItemRemoved -= ViewModel_OnItemRemoved;
            
            viewModel.TargetFilterAdded -= ViewModel_OnTargetFilterAdded;
            viewModel.TargetFilterRemoved -= ViewModel_OnTargetFilterRemoved;
            viewModel.TargetFilterNameChanged -= ViewModel_OnTargetFilterNameChanged;
        }

        private void ViewModel_OnTargetFilterNameChanged(TargetFilterItemViewModel item, string oldName, string newName)
        {
            if (targetFilterField.value == item)
                targetFilterField.SetValueWithoutNotify(item);
        }

        private void ViewModel_OnTargetFilterAdded(TargetFilterItemViewModel targetFilter)
        {
            var choices = targetFilterField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, targetFilter);
        }

        private void ViewModel_OnTargetFilterRemoved(TargetFilterItemViewModel targetFilter)
        {
            var targetFilters = targetFilterField.choices;
            if (targetFilters.Remove(targetFilter))
            {
                if (targetFilterField.value?.Name == targetFilter.Name)
                    targetFilterField.value = null;
            }
        }
        
        private void ViewModel_OnItemAdded(TargetFilterItemViewModelDecisionTab item)
        {
            ValidateButton();
        }

        private void ViewModel_OnItemRemoved(TargetFilterItemViewModelDecisionTab item)
        {
            ValidateButton();
        }

        #endregion
    }
}