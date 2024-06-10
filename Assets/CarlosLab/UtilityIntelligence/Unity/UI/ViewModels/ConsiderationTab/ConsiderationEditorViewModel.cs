#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationEditorViewModel : BaseItemViewModel<ConsiderationModel, ConsiderationEditorListViewModel>,
        INameViewModel, INotifyBindablePropertyChanged
    {
        public event Action<InputItemViewModel> InputChanged;

        public ConsiderationEditorViewModel(IDataAsset asset, ConsiderationModel model) : base(asset,
            model)
        {
            Init();
        }

        #region Normal Properties

        public Type InputValueType => Input?.ValueType;
        
        [CreateProperty]
        public bool HasNoTarget
        {
            get => Model.HasNoTarget;
            set
            {
                if (Model.HasNoTarget == value) return;
                
                Record($"DecisionViewModel HasNoTarget Changed: {value}",
                    () => { Model.HasNoTarget = value; });
            }
        }

        #endregion
        
        #region Fields

        private InputListViewModel inputContainer;
        private InputItemViewModel input;
        private NormalizationCacheViewModel normalizationCache;
        private ConsiderationRuntimeViewModel runtimeViewModel;

        #endregion

        #region Binding Properties

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name == value)
                    return;

                Record($"ConsiderationEditorViewModel Name Changed: {value}",
                    () => { Model.Name = value; });

                Notify();
            }
        }
        


        [CreateProperty]
        public string InputName
        {
            get => Model.InputName ?? "None";
            set
            {
                if (Model.InputName == value)
                    return;

                Record($"ConsiderationEditorViewModel InputName Changed: {value}",
                    () => { Model.InputName = value; });

                Input = InputContainer.GetItemByName(Model.InputName);

                Notify();
            }
        }
        
        [CreateProperty]
        public object RawInput
        {
            get => Input?.ValueObject;
            set
            {
                if (Input != null)
                    Input.ValueObject = value;
            }
        }

        #endregion

        #region ViewModel Properties

        public InputItemViewModel Input
        {
            get
            {
                if (input == null)
                    Input = InputContainer?.GetItemByName(Model.InputName);
                return input;
            }
            private set
            {
                if (input == value)
                    return;

                if (input != null) UnregisterInputEvents(input);
                input = value;
                if (input != null) RegisterInputEvents(input);

                OnInputChanged(input);
            }
        }

        public InputListViewModel InputContainer
        {
            get
            {
                if (inputContainer == null)
                    InputContainer = UtilityIntelligenceEditorUtils.Inputs;

                return inputContainer;
            }
            set
            {
                if (inputContainer == value)
                    return;

                if (inputContainer != null) UnregisterInputContainerEvents(inputContainer);
                inputContainer = value;
                if (inputContainer != null) RegisterInputContainerEvents(inputContainer);
            }
        }

        public NormalizationViewModel Normalization => NormalizationCache.CurrentNormalization;

        public NormalizationCacheViewModel NormalizationCache
        {
            get
            {
                if (normalizationCache == null)
                {
                    normalizationCache = ViewModelFactory<NormalizationCacheViewModel>.Create(Asset, Model);
                    normalizationCache.EditorViewModel = this;
                    normalizationCache.NormalizationChanged += OnNormalizationChanged;
                }

                return normalizationCache;
            }
        }

        public ConsiderationRuntimeViewModel RuntimeViewModel
        {
            get
            {
                if (runtimeViewModel == null)
                {
                    runtimeViewModel = ViewModelFactory<ConsiderationRuntimeViewModel>.Create(Asset, Model);
                    runtimeViewModel.EditorViewModel = this;
                }

                return runtimeViewModel;
            }
        }

        #endregion

        #region ViewModel Methods

        private void Init()
        {
            if (Model == null) return;

            Input = InputContainer.GetItemByName(Model.InputName);
            CalculateConsiderationScore();
        }

        private void RegisterInputEvents(InputItemViewModel input)
        {
            input.ValueChanged += OnInputViewModelValueChanged;
        }

        private void UnregisterInputEvents(InputItemViewModel input)
        {
            input.ValueChanged -= OnInputViewModelValueChanged;
        }
        
        private void UnregisterInputContainerEvents(InputListViewModel viewModel)
        {
            viewModel.ItemNameChanged -= OnInputNameChanged;
            viewModel.ItemRemoved -= OnInputRemoved;
        }

        private void RegisterInputContainerEvents(InputListViewModel viewModel)
        {
            viewModel.ItemNameChanged += OnInputNameChanged;
            viewModel.ItemRemoved += OnInputRemoved;
        }
        

        #endregion

        #region Event Handlers

        protected override void OnModelChanged(ConsiderationModel newModel)
        {
            NormalizationCache.Model = newModel;
            RuntimeViewModel.Model = newModel;

            Notify(nameof(Name));
            Notify(nameof(InputName));
            Notify(nameof(HasNoTarget));

            CalculateConsiderationScore();
        }

        private void OnInputChanged(InputItemViewModel newInput)
        {
            CalculateScore();
            InputChanged?.Invoke(newInput);
        }
        
        private void OnInputNameChanged(InputItemViewModel item, string oldName, string newName)
        {
            if(Input == item)
                InputName = newName;
        }
        
        private void OnInputRemoved(InputItemViewModel item)
        {
            if (Input == item)
            {
                InputName = null;
                NormalizationCache.CurrentNormalization = null;
            }
        }
        

        private void OnNormalizationChanged(NormalizationViewModel newNormalization)
        {
            CalculateScore();
        }

        private void OnInputViewModelValueChanged()
        {
            CalculateScore();
        }

        public void CalculateScore()
        {
            if (Asset.IsRuntimeAsset) return;

            CalculateConsiderationScore();
            MakeDecision();
        }

        private void CalculateConsiderationScore()
        {
            if (Asset.IsRuntimeAsset || Model == null) return;

            var context = ConsiderationContext.Null;
            Model.Runtime.CalculateScore(ref context);
        }

        private void MakeDecision()
        {
            UtilityIntelligenceEditorUtils.Model?.Runtime.MakeDecision(null);
        }

        #endregion
    }
}