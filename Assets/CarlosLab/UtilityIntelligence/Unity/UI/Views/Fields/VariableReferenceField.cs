#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableReferenceField : CustomField<IVariableReference>
    {
        #region Fields

        private Toggle isBlackboardReferenceField;
        private PopupField<VariableViewModel> variableSelectionField;
        private TabView agentTabView;


        #endregion

        #region Events

        public event Action<IVariableReference> ValueChanged;
        public event Action InputApplied;

        #endregion
        
        public VariableReferenceField(string label) : base(label, null)
        {
            valueFieldLabel = label;
            CreateValueFieldContainer();
            CreateVariableSelectionField();
            CreateIsBlackboardReferenceField();

            this.RegisterValueChangedCallback(evt =>
            {
                // UtilityIntelligenceConsole.Instance.Log("VariableReferenceFieldNew ValueChanged IsBlackboardReference: " +
                //           evt.newValue.IsBlackboardReference);
                UpdateView();
            });
        }

        #region Variable

        private static readonly VariableViewModel CreateNewVariable = new(null, null);

        private VariableViewModel variableViewModel;
        
        private VariableViewModel VariableViewModel
        {
            get => variableViewModel;
            set
            {
                if (variableViewModel == value)
                    return;

                if (variableViewModel != null) UnregisterVariableViewModelEvents(variableViewModel);
                variableViewModel = value;
                if (variableViewModel != null) RegisterVariableViewModelEvents(variableViewModel);
            }
        }

        #endregion

        #region Blackboard

        private BlackboardViewModel blackboardViewModel;

        private BlackboardViewModel BlackboardViewModel
        {
            get => blackboardViewModel;
            set
            {
                if (blackboardViewModel == value)
                    return;

                if (blackboardViewModel != null) UnregisterBlackboardViewModelEvents(blackboardViewModel);
                blackboardViewModel = value;
                if (blackboardViewModel != null) RegisterBlackboardViewModelEvents(blackboardViewModel);
                OnBlackboardViewModelChanged(blackboardViewModel);
            }
        }


        #endregion

        private void UpdateView()
        {
            IVariableReference currentValue = value;
            if (currentValue == null)
                return;

            UpdateValueField(currentValue.ValueType, currentValue.ValueObject);
            UpdateVariableSelectionFieldChoices(BlackboardViewModel);
            UpdateIsBlackboardReferenceField(currentValue.IsBlackboardReference);
        }

        #region Value Fields

        private VisualElement valueFieldContainer;
        private ValueField valueField;
        private readonly string valueFieldLabel;
        private Type valueType;

        #endregion

        #region IsBlackboardReferenceField Functions

        private void CreateIsBlackboardReferenceField()
        {
            isBlackboardReferenceField = new Toggle();
            isBlackboardReferenceField.RegisterValueChangedCallback(evt =>
            {
                // UtilityIntelligenceConsole.Instance.Log("IsBlackboardReferenceField ValueChanged: " + evt.newValue);
                value.IsBlackboardReference = evt.newValue;
                UpdateIsBlackboardReferenceField(evt.newValue);

                RaiseValueChanged();
                RaiseInputApplied();
            });
            AddChild(isBlackboardReferenceField);
        }

        private void UpdateIsBlackboardReferenceField(bool isBlackboardReference)
        {
            isBlackboardReferenceField.SetValueWithoutNotify(isBlackboardReference);
            if (isBlackboardReference)
                ShowVariableSelectionField();
            else
                ShowValueField();
        }

        #endregion

        #region VariableSelectionField Functions

        private void CreateVariableSelectionField()
        {
            variableSelectionField = new PopupField<VariableViewModel>();
            variableSelectionField.style.marginLeft = 0;
            FormatVariableSelectionField();
            HandleVariableSelectionFieldValueChanged();

            AddChild(variableSelectionField);
            HideVariableSelectionField();
        }

        private void HandleVariableSelectionFieldValueChanged()
        {
            variableSelectionField.RegisterValueChangedCallback(evt =>
            {
                VariableViewModel newVariable = evt.newValue;

                if (newVariable == CreateNewVariable)
                {
                    agentTabView.selectedTabIndex = IntelligenceViewTabIndexes.BlackboardTab;
                    variableSelectionField.SetValueWithoutNotify(evt.previousValue);
                }
                else if (evt.previousValue != CreateNewVariable)
                {
                    value.Name = newVariable?.Name;
                    VariableViewModel = newVariable;
                    RaiseValueChanged();
                    RaiseInputApplied();
                }
            });
        }

        private void RegisterBlackboardViewModelEvents(BlackboardViewModel viewModel)
        {
            viewModel.ItemAdded += OnVariableAdded;
            viewModel.ItemRemoved += OnVariableRemoved;
            viewModel.ItemNameChanged += OnVariableNameChanged;
        }

        private void UnregisterBlackboardViewModelEvents(BlackboardViewModel viewModel)
        {
            viewModel.ItemAdded -= OnVariableAdded;
            viewModel.ItemRemoved -= OnVariableRemoved;
            viewModel.ItemNameChanged -= OnVariableNameChanged;
        }
        
        private void RegisterVariableViewModelEvents(VariableViewModel viewModel)
        {
            viewModel.ValueChanged += RaiseValueChanged;
        }

        private void UnregisterVariableViewModelEvents(VariableViewModel viewModel)
        {
            viewModel.ValueChanged -= RaiseValueChanged;
        }

        private void OnVariableAdded(VariableViewModel variable)
        {
            if (valueType == variable.ValueType)
                variableSelectionField.choices.Insert(0, variable);
        }

        private void OnVariableRemoved(VariableViewModel variable)
        {
            if (variableSelectionField.value?.Name == variable.Name)
                variableSelectionField.value = null;

            if (valueType == variable.ValueType)
                variableSelectionField.choices.Remove(variable);
        }

        private void OnVariableNameChanged(VariableViewModel item, string oldName, string newName)
        {
            if (variableSelectionField.value != item) return;
            
            value.Name = newName;
            variableSelectionField.SetValueWithoutNotify(item);

            RaiseValueChanged();
            RaiseInputApplied();
        }

        private void UpdateVariableSelectionFieldChoices(BlackboardViewModel viewModel)
        {
            variableSelectionField.choices.Clear();

            var items = viewModel.Items;
            for (int index = 0; index < items.Count; index++)
            {
                VariableViewModel variable = items[index];
                if (valueType == variable.ValueType)
                    variableSelectionField.choices.Add(variable);

                if (variable.Name == value.Name)
                {
                    VariableViewModel = variable;
                    variableSelectionField.SetValueWithoutNotify(variable);
                }
            }

            SortItemReferenceFieldChoices();

            variableSelectionField.choices.Add(CreateNewVariable);
        }

        private void FormatVariableSelectionField()
        {
            variableSelectionField.formatListItemCallback = FormatItem;
            variableSelectionField.formatSelectedValueCallback = FormatItem;

            string FormatItem(VariableViewModel variable)
            {
                if (variable == null)
                    return "NONE";

                if (variable == CreateNewVariable)
                    return "CREATE NEW";

                return variable.Name;
            }
        }

        private void SortItemReferenceFieldChoices()
        {
            variableSelectionField.choices.Sort((a, b) =>
            {
                if (a == null) return -1;

                if (b == null) return 1;
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        #endregion

        #region ValueField Functions

        private void CreateValueFieldContainer()
        {
            valueFieldContainer = new VisualElement();
            valueFieldContainer.style.flexGrow = 1;
            AddChild(valueFieldContainer);
        }

        private void UpdateValueField(Type valueType, object valueObject)
        {
            this.valueType = valueType;
            if (valueField == null)
                valueField = CreateValueField(valueType);

            valueField?.SetValueWithoutNotify(valueObject);
        }

        private ValueField CreateValueField(Type valueType)
        {
            ValueField valueField = new(valueType, false, valueFieldLabel);
            if (valueField.IsValid == false) return null;

            valueField.ClearMarginLeft();

            valueField.ValueChanged += newValue =>
            {
                value.ValueObject = newValue;
                RaiseValueChanged();
            };

            valueField.InputApplied += RaiseInputApplied;

            valueFieldContainer.Add(valueField);

            return valueField;
        }

        #endregion

        #region Show/Hide Fields

        private void ShowVariableSelectionField()
        {
            variableSelectionField.SetDisplay(true);
            labelElement.SetDisplay(true);
            HideAllFieldsExcept(variableSelectionField);
        }

        private void HideVariableSelectionField()
        {
            variableSelectionField.SetDisplay(false);
        }

        private void ShowValueField()
        {
            if (valueField != null)
            {
                labelElement.SetDisplay(false);
                valueFieldContainer.SetDisplay(true);
            }
            else
            {
                labelElement.SetDisplay(true);
            }
            
            HideAllFieldsExcept(valueFieldContainer);
        }

        private void HideValueField()
        {
            valueFieldContainer.SetDisplay(false);
        }

        private void HideAllFieldsExcept(VisualElement field)
        {
            if (valueFieldContainer != field)
                HideValueField();

            if (variableSelectionField != field)
                HideVariableSelectionField();
        }

        #endregion

        #region Event Functions

        private void OnBlackboardMainViewModelChanged(BlackboardViewModel viewModel)
        {
            BlackboardViewModel = viewModel;
        }

        private void OnBlackboardViewModelChanged(BlackboardViewModel viewModel)
        {
            UpdateVariableSelectionFieldChoices(viewModel);
        }

        protected override void OnAttachToPanel(AttachToPanelEvent evt)
        {
            agentTabView = panel.visualTree.Q<TabView>();
            BlackboardTabMainView blackboardTabMainView = panel.visualTree.Q<BlackboardTabMainView>();
            BlackboardViewModel = blackboardTabMainView.ViewModel;
            UpdateView();
            blackboardTabMainView.ViewModelChanged += OnBlackboardMainViewModelChanged;
        }

        #endregion

        #region Raise Event Functions

        private void RaiseValueChanged()
        {
            ValueChanged?.Invoke(value);
        }

        private void RaiseInputApplied()
        {
            InputApplied?.Invoke();
        }

        #endregion
    }
}