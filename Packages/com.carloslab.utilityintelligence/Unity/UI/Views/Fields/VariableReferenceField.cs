#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableReferenceField : RootViewMemberField<IVariableReference, UtilityIntelligenceView>
    {
        #region Fields

        private Toggle isBlackboardReferenceField;
        private PopupField<VariableViewModel> variableSelectionField;

        #endregion

        #region Events

        public event Action<IVariableReference> ValueChanged;
        public event Action InputApplied;

        #endregion
        
        public VariableReferenceField(string label) : base(label, null)
        {
            VisualInput.style.flexGrow = 1;
            valueFieldLabel = label;
            CreateValueFieldContainer();
            CreateVariableSelectionField();
            CreateIsBlackboardReferenceField();

            this.RegisterValueChangedCallback(evt =>
            {
                // UtilityIntelligenceConsole.Instance.Log("VariableReferenceFieldNew ValueChanged IsBlackboardReference: " +
                //           evt.newValue.IsBlackboardReference);
                UpdateView(evt.newValue);
            });
        }

        #region Variable

        private static readonly VariableViewModel CreateNewVariable = new();

        private VariableViewModel variableViewModel;
        
        private VariableViewModel VariableViewModel
        {
            get => variableViewModel;
            set
            {
                if (variableViewModel == value)
                    return;

                UnregisterVariableViewModelEvents(variableViewModel);
                variableViewModel = value;
                RegisterVariableViewModelEvents(variableViewModel);
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

        private void UpdateView(IVariableReference variableReference)
        {
            if (variableReference == null)
                return;

            UpdateValueField(variableReference.ValueType, variableReference.ValueObject);
            UpdateVariableSelectionFieldChoices(BlackboardViewModel);
            UpdateIsBlackboardReferenceField(variableReference.IsBlackboardReference);
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
            variableSelectionField.style.flexGrow = 1;
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
                    RootView.SelectBlackboardTab();
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
            if (viewModel == null)
                return;
            
            viewModel.ValueChanged += RaiseValueChanged;
        }

        private void UnregisterVariableViewModelEvents(VariableViewModel viewModel)
        {
            if (viewModel == null)
                return;
            
            viewModel.ValueChanged -= RaiseValueChanged;
        }

        private void OnVariableAdded(VariableViewModel variable)
        {
            if (variable.ValueType == valueType)
            {
                var choices = variableSelectionField.choices;
                int index = choices.Count - 1;
                choices.Insert(index, variable);
            }
        }

        private void OnVariableRemoved(VariableViewModel variable)
        {
            if (variableSelectionField.value?.Name == variable.Name)
                variableSelectionField.value = null;

            if (variable.ValueType == valueType)
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
                if (variable.ValueType == valueType)
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
                    return "None";

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
                return string.CompareOrdinal(a.Name, b.Name);
            });
        }

        #endregion

        #region ValueField Functions

        private void CreateValueFieldContainer()
        {
            valueFieldContainer = new VisualElement();
            valueFieldContainer.style.flexGrow = 1;
            valueFieldContainer.style.minWidth = 120;
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
                valueFieldContainer.SetDisplay(true);
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
            BlackboardTabMainView blackboardTabMainView = RootView.BlackboardTabMainView;
            BlackboardViewModel = blackboardTabMainView.ViewModel;
            blackboardTabMainView.ViewModelChanged += OnBlackboardMainViewModelChanged;
            UpdateView(value);
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