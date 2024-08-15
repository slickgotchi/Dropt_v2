using System;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionItemCreatorViewIntelligenceTab : ItemCreatorView<DecisionListViewModelIntelligenceTab, DecisionItemViewModelIntelligenceTab>
    {
        private static readonly DecisionItemViewModel CreateNewDecision = new();
        private PopupField<DecisionItemViewModel> decisionField;

        protected override string CreateButtonText { get; } = "Add";
        
        public DecisionItemCreatorViewIntelligenceTab() : base(UIBuilderResourcePaths.ItemReferenceCreatorView)
        {
        }

        #region View Functions
        
        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            
            VisualElement typeFieldContainer = this.Q<VisualElement>("ItemPopupFieldContainer");
            decisionField = CreateDecisionField();
            typeFieldContainer.Add(decisionField);

            FormatDecisionField();
            HandleDecisionFieldValueChanged();
        }

        protected override void OnRefreshView(DecisionListViewModelIntelligenceTab viewModel)
        {
            UpdateDecisionFieldChoices(viewModel.DecisionListViewModel);
        }

        protected override void CreateNewItem()
        {
            var decision = decisionField.value;
            if (decision != null) ViewModel.TryCreateItem(decision.Model, out _);
        }

        #endregion
        
        #region Init DecisionField

        private PopupField<DecisionItemViewModel> CreateDecisionField()
        {
            PopupField<DecisionItemViewModel> decisionField = new();
            decisionField.label = "Name";
            return decisionField;
        }

        private void SortDecisionFieldChoices()
        {
            decisionField.choices.Sort((a, b) =>
            {
                if (a == null) return -1;

                if (b == null) return 1;
                return string.CompareOrdinal(a.Name, b.Name);
            });
        }

        private void FormatDecisionField()
        {
            decisionField.formatListItemCallback = FormatItem;
            decisionField.formatSelectedValueCallback = FormatItem;

            string FormatItem(DecisionItemViewModel decision)
            {
                if (decision == null)
                    return "None";

                if (decision == CreateNewDecision)
                    return "CREATE NEW";

                return decision.Name;
            }
        }

        private void HandleDecisionFieldValueChanged()
        {
            decisionField.RegisterValueChangedCallback(evt =>
            {
                var newDecision = evt.newValue;

                if (newDecision == CreateNewDecision)
                {
                    RootView.SelectDecisionTab();

                    decisionField.SetValueWithoutNotify(evt.previousValue);
                }

                ValidateButton();
            });
        }

        #endregion
        
        #region Update DecisionField

        private void UpdateDecisionFieldChoices(DecisionListViewModel editorListViewModel)
        {
            if (editorListViewModel == null) return;

            decisionField.SetValueWithoutNotify(null);
            decisionField.choices.Clear();

            var decisions = editorListViewModel.Items;
            for (int index = 0; index < decisions.Count; index++)
            {
                var decision = decisions[index];
                decisionField.choices.Add(decision);
            }
            //
            // if (decisionField.choices.Count > 0 && decisionField.value == null)
            //     decisionField.value = decisionField.choices[0];

            decisionField.choices.Add(CreateNewDecision);
        }

        #endregion
        
        #region Helper Functions

        private void ValidateButton()
        {
            var decision = decisionField.value;
            string name = decision?.Name;
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
        
        #region ViewModel Events

        protected override void OnRegisterViewModelEvents(DecisionListViewModelIntelligenceTab viewModel)
        {
            viewModel.ItemAdded += ViewModel_OnItemAdded;
            viewModel.ItemRemoved += ViewModel_OnItemRemoved;
            
            viewModel.DecisionAdded += ViewModel_OnDecisionAdded;
            viewModel.DecisionRemoved += ViewModel_OnDecisionRemoved;
            viewModel.DecisionNameChanged += ViewModel_OnDecisionNameChanged;
        }

        protected override void OnUnregisterViewModelEvents(DecisionListViewModelIntelligenceTab viewModel)
        {
            viewModel.ItemAdded -= ViewModel_OnItemAdded;
            viewModel.ItemRemoved -= ViewModel_OnItemRemoved;
            
            viewModel.DecisionAdded -= ViewModel_OnDecisionAdded;
            viewModel.DecisionRemoved -= ViewModel_OnDecisionRemoved;
            viewModel.DecisionNameChanged -= ViewModel_OnDecisionNameChanged;
        }

        private void ViewModel_OnDecisionNameChanged(DecisionItemViewModel item, string oldName, string newName)
        {
            if (decisionField.value == item)
                decisionField.SetValueWithoutNotify(item);
        }

        private void ViewModel_OnDecisionAdded(DecisionItemViewModel decision)
        {
            var choices = decisionField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, decision);
        }

        private void ViewModel_OnDecisionRemoved(DecisionItemViewModel decision)
        {
            var decisions = decisionField.choices;
            if (decisions.Remove(decision))
            {
                if (decisionField.value?.Name == decision.Name)
                    decisionField.value = null;
            }
        }

        private void ViewModel_OnItemAdded(DecisionItemViewModelIntelligenceTab item)
        {
            ValidateButton();
            MakeDecision();
        }

        private void ViewModel_OnItemRemoved(DecisionItemViewModelIntelligenceTab item)
        {
            ValidateButton();

            MakeDecision();
        }

        #endregion
    }
}