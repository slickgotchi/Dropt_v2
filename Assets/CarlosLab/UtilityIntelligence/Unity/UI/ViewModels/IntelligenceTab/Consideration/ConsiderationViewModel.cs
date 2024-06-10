#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationViewModel : BaseItemViewModel<ConsiderationModel, ConsiderationListViewModel>,
        INameViewModel, IStatusViewModel, INotifyBindablePropertyChanged
    {
        private ConsiderationRuntimeContextViewModel contextViewModel;
        private ConsiderationEditorViewModel editorViewModel;

        public ConsiderationViewModel(IDataAsset asset, ConsiderationModel model) : base(asset, model)
        {
        }

        public Type InputValueType => EditorViewModel.Input?.ValueType;

        #region ViewModel Properties

        public ConsiderationRuntimeContextViewModel ContextViewModel
        {
            get
            {
                if (contextViewModel == null)
                {
                    contextViewModel = ViewModelFactory<ConsiderationRuntimeContextViewModel>.Create(Asset, Model);
                    contextViewModel.ConsiderationListViewModel = ListViewModel;
                    contextViewModel.ConsiderationViewModel = this;
                    
                    RegisterContextViewModelEvents(contextViewModel);
                }

                return contextViewModel;
            }
        }

        public ConsiderationEditorViewModel EditorViewModel
        {
            get => editorViewModel;
            set
            {
                if (editorViewModel == value)
                    return;

                if (editorViewModel != null)
                    UnregisterEditorViewModelEvents(editorViewModel);

                editorViewModel = value;

                if (editorViewModel != null)
                    RegisterEditorViewModelEvents(editorViewModel);
            }
        }

        public ConsiderationRuntimeViewModel RuntimeViewModel => EditorViewModel?.RuntimeViewModel;

        #endregion

        #region Binding Properties

        [CreateProperty]
        public string Name
        {
            get => editorViewModel.Name;
            set => editorViewModel.Name = value;
        }

        [CreateProperty] public string InputName => EditorViewModel.InputName;

        [CreateProperty] public string NormalizationName => EditorViewModel.Normalization?.Name ?? "None";

        public NormalizationViewModel Normalization => EditorViewModel.Normalization;

        #endregion

        #region Register/Unregister Events

        private void RegisterEditorViewModelEvents(ConsiderationEditorViewModel viewModel)
        {
            viewModel.propertyChanged += ConsiderationEditorViewModel_PropertyChanged;
            viewModel.NormalizationCache.NormalizationChanged += NormalizationCache_NormalizationChanged;
        }

        private void UnregisterEditorViewModelEvents(ConsiderationEditorViewModel viewModel)
        {
            viewModel.propertyChanged -= ConsiderationEditorViewModel_PropertyChanged;
            viewModel.NormalizationCache.NormalizationChanged -= NormalizationCache_NormalizationChanged;
        }
        
        private void RegisterContextViewModelEvents(ConsiderationRuntimeContextViewModel viewModel)
        {
            viewModel.propertyChanged += ConsiderationRuntimeContextViewModel_PropertyChanged;
        }

        #endregion

        #region Event Handlers

        private void ConsiderationEditorViewModel_PropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(e.propertyName);
        }

        private void NormalizationCache_NormalizationChanged(NormalizationViewModel normalization)
        {
            Notify(nameof(NormalizationName));
        }
        
        private void ConsiderationRuntimeContextViewModel_PropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if(e.propertyName == nameof(CurrentStatus))
                StatusChanged?.Invoke(CurrentStatus);
        }

        #endregion

        #region Status
        
        public event Action<Status> StatusChanged;
        
        public Status CurrentStatus => (Status) ContextViewModel.CurrentStatus;
        
        #endregion
        
        protected override void OnModelChanged(ConsiderationModel newModel)
        {
            ContextViewModel.Model = newModel;
        }

    }
}