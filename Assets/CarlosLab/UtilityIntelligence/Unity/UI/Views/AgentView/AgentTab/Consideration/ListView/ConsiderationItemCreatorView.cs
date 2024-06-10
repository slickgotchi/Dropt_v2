#region

using System;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationItemCreatorView
        : ItemCreatorView<ConsiderationListViewModel, ConsiderationViewModel>
    {
        private static readonly ConsiderationEditorViewModel CreateNewConsideration = new(null, null);
        private PopupField<ConsiderationEditorViewModel> considerationField;

        private ConsiderationEditorListViewModel editorViewModel;

        protected override string CreateButtonText { get; } = "Add";

        public ConsiderationItemCreatorView() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            
            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");
            considerationField = CreateConsiderationField();
            typeFieldContainer.Add(considerationField);

            FormatConsiderationField();
            HandleConsiderationFieldValueChanged();
        }

        private ConsiderationEditorListViewModel EditorViewModel
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

        protected override void OnUpdateView(ConsiderationListViewModel viewModel)
        {
            EditorViewModel = UtilityIntelligenceEditorUtils.Considerations;
            UpdateConsiderationFieldChoices(EditorViewModel);
        }

        protected void ValidateButton()
        {
            ConsiderationEditorViewModel consideration = considerationField.value;
            string name = consideration?.Name;
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
            ConsiderationEditorViewModel consideration = considerationField.value;
            if (consideration != null) ViewModel.TryCreateItem(consideration.Model, out _);
        }

        #region Update ConsiderationField

        private void UpdateConsiderationFieldChoices(ConsiderationEditorListViewModel editorListViewModel)
        {
            if (editorListViewModel == null) return;

            considerationField.SetValueWithoutNotify(null);
            considerationField.choices.Clear();

            var considerations = editorListViewModel.Items;
            for (int index = 0; index < considerations.Count; index++)
            {
                ConsiderationEditorViewModel consideration = considerations[index];
                considerationField.choices.Add(consideration);
            }

            if (considerationField.choices.Count > 0 && considerationField.value == null)
                considerationField.value = considerationField.choices[0];

            considerationField.choices.Add(CreateNewConsideration);
        }

        #endregion

        #region Init ConsiderationField

        private PopupField<ConsiderationEditorViewModel> CreateConsiderationField()
        {
            PopupField<ConsiderationEditorViewModel> considerationField = new();
            considerationField.label = "Name";
            return considerationField;
        }

        private void SortConsiderationFieldChoices()
        {
            considerationField.choices.Sort((a, b) =>
            {
                if (a == null) return -1;

                if (b == null) return 1;
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        private void FormatConsiderationField()
        {
            considerationField.formatListItemCallback = FormatItem;
            considerationField.formatSelectedValueCallback = FormatItem;

            string FormatItem(ConsiderationEditorViewModel consideration)
            {
                if (consideration == null)
                    return "NONE";

                if (consideration == CreateNewConsideration)
                    return "CREATE NEW";

                return consideration.Name;
            }
        }

        private void HandleConsiderationFieldValueChanged()
        {
            considerationField.RegisterValueChangedCallback(evt =>
            {
                ConsiderationEditorViewModel newConsideration = evt.newValue;

                if (newConsideration == CreateNewConsideration)
                {
                    MainTabView.selectedTabIndex = IntelligenceViewTabIndexes.ConsiderationTab;

                    considerationField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateButton();
            });
        }

        #endregion

        #region ViewModel Events

        private void UnregisterEditorViewModelEvents(ConsiderationEditorListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnEditorItemAdded;
            viewModel.ItemRemoved -= OnEditorItemRemoved;
            viewModel.ItemNameChanged -= OnEditorItemNameChanged;
            viewModel.ModelChanged -= OnEditorModelChanged;
        }

        private void RegisterEditorViewModelEvents(ConsiderationEditorListViewModel viewModel)
        {
            viewModel.ItemAdded += OnEditorItemAdded;
            viewModel.ItemRemoved += OnEditorItemRemoved;
            viewModel.ItemNameChanged += OnEditorItemNameChanged;
            viewModel.ModelChanged += OnEditorModelChanged;
        }

        protected override void OnRegisterViewModelEvents(ConsiderationListViewModel viewModel)
        {
            viewModel.ItemAdded += OnItemAdded;
            viewModel.ItemRemoved += OnItemRemoved;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnItemAdded;
            viewModel.ItemRemoved -= OnItemRemoved;
        }

        private void OnEditorItemNameChanged(ConsiderationEditorViewModel item, string oldName, string newName)
        {
            if (considerationField.value == item)
                considerationField.SetValueWithoutNotify(item);
        }

        private void OnEditorItemAdded(ConsiderationEditorViewModel consideration)
        {
            considerationField.choices.Insert(0, consideration);
        }

        private void OnEditorItemRemoved(ConsiderationEditorViewModel consideration)
        {
            if (considerationField.value?.Name == consideration.Name)
                considerationField.value = null;

            considerationField.choices.Remove(consideration);
        }

        private void OnItemAdded(ConsiderationViewModel item)
        {
            ValidateButton();
            MakeDecision();
        }

        private void OnItemRemoved(ConsiderationViewModel item)
        {
            ValidateButton();

            MakeDecision();
        }
        
        public void MakeDecision()
        {
            var asset = UtilityIntelligenceEditorUtils.Asset;
            var intelligenceModel = UtilityIntelligenceEditorUtils.Model;
            if (!asset.IsRuntimeAsset && intelligenceModel != null)
                intelligenceModel.Runtime.MakeDecision(null);
        }

        private void OnEditorModelChanged()
        {
            if (considerationField.value != null)
                considerationField.SetValueWithoutNotify(considerationField.value);
        }

        #endregion
    }
}