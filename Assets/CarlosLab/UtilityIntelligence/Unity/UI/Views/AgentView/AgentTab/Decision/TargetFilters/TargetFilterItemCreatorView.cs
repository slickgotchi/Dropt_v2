#region

using System;
using System.Collections.Generic;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterItemCreatorView : ItemCreatorView<TargetFilterListViewModel, TargetFilterViewModel>
    {
        private static readonly TargetFilterEditorViewModel CreateNewTargetFilter = new(null, null);
        private PopupField<TargetFilterEditorViewModel> targetFilterField;

        private TargetFilterEditorListViewModel editorViewModel;

        protected override string CreateButtonText { get; } = "Add";

        public TargetFilterItemCreatorView() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");
            
            targetFilterField = CreateTargetFilterField();
            typeFieldContainer.Add(targetFilterField);

            FormatTargetFilterField();
            HandleTargetFilterFieldValueChanged();
        }

        private TargetFilterEditorListViewModel EditorViewModel
        {
            get => editorViewModel;
            set
            {
                if (editorViewModel == value)
                    return;

                if (editorViewModel != null) UnregisterEditorViewModelEvents(editorViewModel);
                editorViewModel = value;
                if (editorViewModel != null) RegisterEditorViewModelEvents(editorViewModel);
            }
        }

        protected TabView MainTabView { get; private set; }

        protected override void OnAttachToPanel(AttachToPanelEvent evt)
        {
            MainTabView = panel.visualTree.Q<TabView>();
        }

        protected override void OnUpdateView(TargetFilterListViewModel viewModel)
        {
            EditorViewModel = UtilityIntelligenceEditorUtils.TargetFilters;
            UpdateTargetFilterFieldChoices(EditorViewModel);
        }

        protected void ValidateButton()
        {
            TargetFilterEditorViewModel targetFilter = targetFilterField.value;
            string name = targetFilter?.Name;
            bool isValidated = ValidateName(name);
            createButton.SetEnabled(isValidated);
        }

        protected bool ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            bool nameExists = ViewModel.Contains(name);
            return !nameExists;
        }


        protected override void CreateNewItem()
        {
            TargetFilterEditorViewModel targetFilter = targetFilterField.value;
            if (targetFilter != null) ViewModel.TryCreateItem(targetFilter.Model, out _);
        }

        #region Update TargetFilterField

        private void UpdateTargetFilterFieldChoices(TargetFilterEditorListViewModel targetFiltersViewModel)
        {
            if (targetFiltersViewModel == null) return;

            targetFilterField.SetValueWithoutNotify(null);
            targetFilterField.choices.Clear();

            var targetFilters = targetFiltersViewModel.Items;
            for (int index = 0; index < targetFilters.Count; index++)
            {
                TargetFilterEditorViewModel targetFilter = targetFilters[index];
                targetFilterField.choices.Add(targetFilter);
            }

            if (targetFilterField.choices.Count > 0 && targetFilterField.value == null)
                targetFilterField.value = targetFilterField.choices[0];

            targetFilterField.choices.Add(CreateNewTargetFilter);
        }

        #endregion

        #region Init TargetFilterField

        private PopupField<TargetFilterEditorViewModel> CreateTargetFilterField()
        {
            PopupField<TargetFilterEditorViewModel> targetFilterField = new();
            targetFilterField.label = "Name";
            return targetFilterField;
        }

        private void SortTargetFilterFieldChoices()
        {
            targetFilterField.choices.Sort((a, b) =>
            {
                if (a == null) return -1;

                if (b == null) return 1;
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        private void FormatTargetFilterField()
        {
            targetFilterField.formatListItemCallback = FormatItem;
            targetFilterField.formatSelectedValueCallback = FormatItem;

            string FormatItem(TargetFilterEditorViewModel targetFilter)
            {
                if (targetFilter == null)
                    return "NONE";

                if (targetFilter == CreateNewTargetFilter)
                    return "CREATE NEW";

                return targetFilter.Name;
            }
        }

        private void HandleTargetFilterFieldValueChanged()
        {
            targetFilterField.RegisterValueChangedCallback(evt =>
            {
                TargetFilterEditorViewModel newTargetFilter = evt.newValue;

                if (newTargetFilter == CreateNewTargetFilter)
                {
                    MainTabView.selectedTabIndex = IntelligenceViewTabIndexes.TargetFilterTab;

                    targetFilterField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateButton();
            });
        }

        #endregion

        #region ViewModel Events

        private void UnregisterEditorViewModelEvents(TargetFilterEditorListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnEditorItemAdded;
            viewModel.ItemRemoved -= OnEditorItemRemoved;
            viewModel.ItemNameChanged -= OnEditorItemNameChanged;
            viewModel.ModelChanged -= OnEditorModelChanged;
        }

        private void RegisterEditorViewModelEvents(TargetFilterEditorListViewModel viewModel)
        {
            viewModel.ItemAdded += OnEditorItemAdded;
            viewModel.ItemRemoved += OnEditorItemRemoved;
            viewModel.ItemNameChanged += OnEditorItemNameChanged;
            viewModel.ModelChanged += OnEditorModelChanged;
        }

        protected override void OnRegisterViewModelEvents(TargetFilterListViewModel viewModel)
        {
            viewModel.ItemAdded += OnItemAdded;
            viewModel.ItemRemoved += OnItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(TargetFilterListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnItemAdded;
            viewModel.ItemRemoved -= OnItemRemoved;
        }

        private void OnEditorItemNameChanged(TargetFilterEditorViewModel item, string oldName, string newName)
        {
            if (targetFilterField.value == item)
                targetFilterField.SetValueWithoutNotify(item);
        }

        private void OnEditorItemAdded(TargetFilterEditorViewModel targetFilter)
        {
            targetFilterField.choices.Insert(0, targetFilter);
        }

        private void OnEditorItemRemoved(TargetFilterEditorViewModel targetFilter)
        {
            if (targetFilterField.value?.Name == targetFilter.Name)
                targetFilterField.value = null;

            targetFilterField.choices.Remove(targetFilter);
        }
        
        private void OnEditorModelChanged()
        {
            if (targetFilterField.value != null)
                targetFilterField.SetValueWithoutNotify(targetFilterField.value);
        }

        private void OnItemAdded(TargetFilterViewModel item)
        {
            ValidateButton();
        }

        private void OnItemRemoved(TargetFilterViewModel item)
        {
            ValidateButton();
        }

        #endregion
    }
}