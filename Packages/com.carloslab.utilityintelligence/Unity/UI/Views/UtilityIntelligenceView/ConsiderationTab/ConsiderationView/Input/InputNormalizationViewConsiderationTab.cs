using System;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class InputNormalizationViewConsiderationTab : UtilityIntelligenceViewMember<ConsiderationItemViewModel>
    {
        public InputNormalizationViewConsiderationTab() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input Normalization";
            Add(foldout);
            
            CreateInputNormalizationField(foldout);
            
            CreateNormalizedInputField(foldout);
            
            CreateInputView();
        }
        

        #region InputNormalizationView

        protected override void OnUpdateView(ConsiderationItemViewModel viewModel)
        {
            inputView.UpdateView(viewModel);
        }

        protected override void OnEnableRuntimeMode()
        {
            inputNormalizationField.SetEnabled(false);
        }

        protected override void OnEnableEditMode()
        {
            normalizedInputField.SetDataBinding(nameof(FloatField.value),
                nameof(ConsiderationItemViewModel.NormalizedInput), BindingMode.ToTarget);
        }

        protected override void OnRefreshView(ConsiderationItemViewModel viewModel)
        {
            UpdateInputNormalizationFieldChoices(viewModel.InputNormalizationListViewModel);
            UpdateNormalizedInputDisplay(viewModel.InputNormalizationViewModel);
        }

        protected override void OnResetView()
        {
            normalizedInputField.ClearBindings();
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            inputView.RootView = rootView;
        }

        protected override void OnModelChanged(IModel newModel)
        {
            if (newModel == null) return;
            
            UpdateInputNormalizationFieldChoices(ViewModel.InputNormalizationListViewModel);
        }

        #endregion

        #region InputView
        
        private InputViewConsiderationTab inputView;

        private void CreateInputView()
        {
            inputView = new();
            inputView.style.marginTop = 5;
            Add(inputView);
        }

        #endregion

        #region NormalizedInputField
        
        private FloatField normalizedInputField;

        private void CreateNormalizedInputField(Foldout foldout)
        {
            normalizedInputField = new("Normalized Input");
            normalizedInputField.SetEnabled(false);
            foldout.Add(normalizedInputField);
        }
        
        private void UpdateNormalizedInputDisplay(InputNormalizationItemViewModel viewModel)
        {
            if (viewModel != null)
                normalizedInputField.SetDisplay(true);
            else
                normalizedInputField.SetDisplay(false);
        }
        
        #endregion

        #region Init InputNormalizationField
        
        private static readonly InputNormalizationItemViewModel CreateNewInputNormalization = new();
        
        private PopupField<InputNormalizationItemViewModel> inputNormalizationField;

        private void CreateInputNormalizationField(Foldout foldout)
        {
            inputNormalizationField = new();
            inputNormalizationField.label = "Name";
            FormatInputNormalizationField();
            HandleInputFieldValueChanged();
            foldout.Add(inputNormalizationField);
        }
        
        private void FormatInputNormalizationField()
        {
            inputNormalizationField.formatListItemCallback = FormatSelectedItem;
            inputNormalizationField.formatSelectedValueCallback = FormatSelectedItem;

            // string FormatListItem(InputNormalizationItemViewModel inputNormalization)
            // {
            //     if (inputNormalization == null)
            //         return "None";
            //
            //     if (inputNormalization == CreateNewInputNormalization)
            //         return "CREATE NEW";
            //
            //     string category = inputNormalization.Category;
            //     if (!string.IsNullOrEmpty(category))
            //         return $"{category}/{inputNormalization.Name}";
            //
            //     Type inputValueType = inputNormalization.InputValueType;
            //     string inputValueTypeName = inputValueType.GetName();
            //
            //     return $"{inputValueTypeName}/{inputNormalization.Name}";
            // }

            string FormatSelectedItem(InputNormalizationItemViewModel inputNormalization)
            {
                if (inputNormalization == null)
                    return "None";

                if (inputNormalization == CreateNewInputNormalization)
                    return "CREATE NEW";

                return inputNormalization.Name;
            }
        }
        
        private void HandleInputFieldValueChanged()
        {
            inputNormalizationField.RegisterValueChangedCallback(evt =>
            {
                InputNormalizationItemViewModel newInput = evt.newValue;

                var considerationViewModel = ViewModel;

                if (newInput == CreateNewInputNormalization)
                {
                    RootView.SelectInputNormalizationTab();

                    inputNormalizationField.SetValueWithoutNotify(evt.previousValue);
                }
                else if (evt.previousValue != CreateNewInputNormalization)
                {
                    considerationViewModel.InputNormalizationName = newInput?.Name;
                    inputView.UpdateView(considerationViewModel);
                }
                
                UpdateNormalizedInputDisplay(considerationViewModel.InputNormalizationViewModel);
            });
        }

        #endregion
        
        #region Update InputNormalizationField

        private void UpdateInputNormalizationFieldChoices(InputNormalizationListViewModel normalizationContainer)
        {
            if (normalizationContainer == null) return;
            
            ResetInputNormalizationFieldWithoutNotify();
            // inputView.Reset

            inputNormalizationField.choices.Add(null);

            var inputs = normalizationContainer.Items;
            for (int index = 0; index < inputs.Count; index++)
            {
                var inputNormalization = inputs[index];
                
                inputNormalizationField.choices.Add(inputNormalization);

                if (inputNormalization.Name == ViewModel.Model.InputNormalizationName)
                    inputNormalizationField.value = inputNormalization;
            }

            inputNormalizationField.choices.Sort(CompareChoices);

            int CompareChoices(InputNormalizationItemViewModel choice1, InputNormalizationItemViewModel choice2)
            {
                if (choice1 == null)
                    return -1;
                if (choice2 == null)
                    return 1;
                
                return string.CompareOrdinal(choice1.Name, choice2.Name);
                
                // int result;
                //
                // var category1 = choice1.Category;
                // var category2 = choice2.Category;
                // if (string.IsNullOrEmpty(category1) && string.IsNullOrEmpty(category2) )
                // {
                //     Type inputValueType1 = choice1.InputValueType;
                //     Type inputValueType2 = choice2.InputValueType;
                //
                //     string inputValueTypeName1 = inputValueType1.GetName();
                //     string inputValueTypeName2 = inputValueType2.GetName();
                //
                //     result = string.CompareOrdinal(inputValueTypeName1, inputValueTypeName2);
                //     if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
                //
                //     return result;
                // }
                //
                // if (string.IsNullOrEmpty(category1))
                //     return 1;
                //
                // if (string.IsNullOrEmpty(category2))
                //     return -1;
                //
                // result = string.CompareOrdinal(category1, category2);
                // if (result == 0) return string.CompareOrdinal(choice1.Name, choice2.Name);
                // return result;
            }

            inputNormalizationField.choices.Add(CreateNewInputNormalization);
        }
        
        public void ResetInputNormalizationField()
        {
            inputNormalizationField.value = null;
            inputNormalizationField.choices.Clear();
        }

        public void ResetInputNormalizationFieldWithoutNotify()
        {
            inputNormalizationField.SetValueWithoutNotify(null);
            inputNormalizationField.choices.Clear();
        }

        #endregion

        #region ViewModel Events
        
        protected override void OnRegisterViewModelEvents(ConsiderationItemViewModel viewModel)
        {
            viewModel.InputNormalizationAdded += ViewModel_OnInputNormalizationAdded;
            viewModel.InputNormalizationRemoved += ViewModel_OnInputNormalizationRemoved;
            viewModel.InputNormalizationNameChanged += ViewModel_OnInputNormalizationNameChanged;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationItemViewModel viewModel)
        {
            viewModel.InputNormalizationAdded -= ViewModel_OnInputNormalizationAdded;
            viewModel.InputNormalizationRemoved -= ViewModel_OnInputNormalizationRemoved;
            viewModel.InputNormalizationNameChanged -= ViewModel_OnInputNormalizationNameChanged;
        }

        private void ViewModel_OnInputNormalizationNameChanged(InputNormalizationItemViewModel inputNormalization, string oldName, string newName)
        {
            if (inputNormalizationField.value == inputNormalization)
                inputNormalizationField.SetValueWithoutNotify(inputNormalization);
        }

        private void ViewModel_OnInputNormalizationAdded(InputNormalizationItemViewModel inputNormalization)
        {
            var choices = inputNormalizationField.choices;
            int index = choices.Count - 1;
            choices.Insert(index, inputNormalization);
        }

        private void ViewModel_OnInputNormalizationRemoved(InputNormalizationItemViewModel inputNormalization)
        {
            if (inputNormalizationField.value == inputNormalization)
                inputNormalizationField.value = null;

            inputNormalizationField.choices.Remove(inputNormalization);
        }

        #endregion
    }
}