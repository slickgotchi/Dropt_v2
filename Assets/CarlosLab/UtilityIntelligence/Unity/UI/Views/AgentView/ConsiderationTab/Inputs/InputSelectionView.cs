#region

using System;
using CarlosLab.Common.Extensions;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class InputSelectionView : BaseView<ConsiderationEditorViewModel>
    {
        private static readonly InputItemViewModel CreateNewInput = new(null, null);

        private readonly NormalizationSelectionView normalizationView;
        public NormalizationSelectionView NormalizationView => NormalizationView;
        
        private InputListViewModel inputContainer;

        private PopupField<InputItemViewModel> inputField;
        private InputSelectionValueView inputValueView;

        public InputSelectionView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input";
            Add(foldout);

            CreateInputField(foldout);

            CreateInputValueField(foldout);

            normalizationView = new NormalizationSelectionView();
            normalizationView.style.marginTop = 5;
            Add(normalizationView);
        }

        private InputListViewModel InputContainer
        {
            get => inputContainer;
            set
            {
                if (inputContainer == value)
                    return;

                if (inputContainer != null) UnregisterInputListViewModelEvents(inputContainer);
                inputContainer = value;
                if (inputContainer != null) RegisterInputListViewModelEvents(inputContainer);
            }
        }

        protected override void OnUpdateView(ConsiderationEditorViewModel viewModel)
        {
            inputValueView.UpdateView(viewModel.RuntimeViewModel);
            normalizationView.UpdateView(viewModel.NormalizationCache);

            UpdateInputFieldChoices(inputContainer);
        }

        private void CreateInputValueField(Foldout foldout)
        {
            inputValueView = new InputSelectionValueView();
            foldout.Add(inputValueView);
        }

        #region Update InputField

        private void UpdateInputFieldChoices(InputListViewModel inputsViewModel)
        {
            if (inputsViewModel == null) return;

            ResetInputFieldWithoutNotify();
            normalizationView.ResetWithoutNotify();

            inputField.choices.Add(null);

            var inputs = inputsViewModel.Items;
            for (int index = 0; index < inputs.Count; index++)
            {
                InputItemViewModel input = inputs[index];
                inputField.choices.Add(input);

                if (input.Name == ViewModel.Model.InputName)
                    inputField.value = input;
            }

            //SortInputFieldChoices();

            inputField.choices.Add(CreateNewInput);
        }

        private void ResetInputFieldWithoutNotify()
        {
            inputField.SetValueWithoutNotify(null);
            inputField.choices.Clear();
        }

        #endregion

        protected override void OnViewModelChanged(ConsiderationEditorViewModel viewModel)
        {
            InputContainer = UtilityIntelligenceEditorUtils.Inputs;
        }

        #region Init InputField

        private void CreateInputField(Foldout foldout)
        {
            inputField = new PopupField<InputItemViewModel>();
            inputField.label = "Name";
            FormatInputField();
            HandleInputFieldValueChanged();
            foldout.Add(inputField);
        }

        private void SortInputFieldChoices()
        {
            inputField.choices.Sort((a, b) =>
            {
                if (a == null) return -1;

                if (b == null) return 1;
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        private void FormatInputField()
        {
#if UNITY_6000_0_OR_NEWER
            inputField.formatListItemCallback = FormatItemGroupByType;
            
            string FormatItemGroupByType(InputItemViewModel input)
            {
                if (input == null)
                    return "None";

                if (input == CreateNewInput)
                    return "CREATE NEW";

                Type inputValueType = input.ValueType;
                string inputValueTypeName = inputValueType.GetName();

                return $"{inputValueTypeName}/{input.Name}";
            }
#else
            inputField.formatListItemCallback = FormatItemUsingName;
#endif
            inputField.formatSelectedValueCallback = FormatItemUsingName;
            
            string FormatItemUsingName(InputItemViewModel input)
            {
                if (input == null)
                    return "None";

                if (input == CreateNewInput)
                    return "CREATE NEW";

                return input.Name;
            }
        }

        private void HandleInputFieldValueChanged()
        {
            inputField.RegisterValueChangedCallback(evt =>
            {
                InputItemViewModel newInput = evt.newValue;

                if (newInput == CreateNewInput)
                {
                    UtilityIntelligenceEditorUtils.TabView.selectedTabIndex = IntelligenceViewTabIndexes.InputTab;

                    inputField.SetValueWithoutNotify(evt.previousValue);
                }
                else if (evt.previousValue != CreateNewInput)
                {
                    if (newInput != null)
                    {
                        ViewModel.InputName = newInput.Name;

                        Type inputValueType = ViewModel.Input.ValueType;
                        ViewModel.NormalizationCache.AddInputValueCache(inputValueType);
                        normalizationView.UpdateNormalizationField(inputValueType);

                        inputValueView.UpdateValueField(ViewModel);
                    }
                    else
                    {
                        ViewModel.InputName = null;
                        inputValueView.Clear();
                        normalizationView.Reset();
                    }
                }
            });
        }

        #endregion

        #region ViewModel Events

        protected override void OnModelChanged()
        {
            UpdateInputFieldChoices(inputContainer);
        }

        private void UnregisterInputListViewModelEvents(InputListViewModel viewModel)
        {
            viewModel.ItemAdded -= OnItemAdded;
            viewModel.ItemRemoved -= OnItemRemoved;

            viewModel.ItemNameChanged -= OnItemNameChanged;
        }

        private void RegisterInputListViewModelEvents(InputListViewModel viewModel)
        {
            viewModel.ItemAdded += OnItemAdded;
            viewModel.ItemRemoved += OnItemRemoved;

            viewModel.ItemNameChanged += OnItemNameChanged;
        }

        private void OnItemNameChanged(InputItemViewModel item, string oldName, string newName)
        {
            if (inputField.value == item)
                inputField.SetValueWithoutNotify(item);
        }

        private void OnItemAdded(InputItemViewModel input)
        {
            inputField.choices.Insert(1, input);
        }

        private void OnItemRemoved(InputItemViewModel input)
        {
            if (inputField.value == input)
                inputField.value = null;

            inputField.choices.Remove(input);
        }

        #endregion
    }
}