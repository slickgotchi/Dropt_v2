#region

using System;
using System.Collections.Generic;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class NormalizationCacheViewModel : ViewModel<ConsiderationModel>
    {
        //Key1: InputValueType, Key2: NormalizationType
        private readonly Dictionary<Type, Dictionary<Type, NormalizationViewModel>> normalizationCache = new();
        private NormalizationViewModel currentNormalization;
        public ConsiderationEditorViewModel EditorViewModel;

        public NormalizationCacheViewModel(IDataAsset asset, ConsiderationModel model) : base(asset,
            model)
        {
            if (model.InputNormalization != null) currentNormalization = CreateViewModel(model.InputNormalization);
        }

        public NormalizationViewModel CurrentNormalization
        {
            get => currentNormalization;
            set
            {
                if (currentNormalization == value) return;

                currentNormalization = value;

                Record("NormalizationCacheViewModel Normalization Changed",
                    () => { Model.InputNormalization = currentNormalization?.Model; });

                NormalizationChanged?.Invoke(currentNormalization);
            }
        }

        public event Action<NormalizationViewModel> NormalizationChanged;

        public void UpdateNormalizationModel(Type inputNormalizationType, Type inputValueType)
        {
            if (CurrentNormalization == null ||
                CurrentNormalization.RuntimeType != inputNormalizationType)
                CurrentNormalization = GetInputNormalization(inputNormalizationType, inputValueType);
        }

        private NormalizationViewModel GetInputNormalization(Type inputNormalizationType, Type inputValueType)
        {
            Dictionary<Type, NormalizationViewModel> typeCache = normalizationCache[inputValueType];
            if (typeCache.TryGetValue(inputNormalizationType, out NormalizationViewModel value))
                return value;
            NormalizationViewModel input = CreateViewModel(inputNormalizationType);
            return input;
        }

        private NormalizationViewModel CreateViewModel(Type inputType)
        {
            InputNormalizationModel model = CreateModel(inputType);
            NormalizationViewModel viewModel = CreateViewModel(model);
            return viewModel;
        }

        private NormalizationViewModel CreateViewModel(InputNormalizationModel model)
        {
            NormalizationViewModel viewModel = ViewModelFactory<NormalizationViewModel>.Create(Asset, model);
            viewModel.Name = model.RuntimeType.Name;
            AddNormalizationToCache(viewModel);
            return viewModel;
        }

        private InputNormalizationModel CreateModel(Type inputNormalizationType)
        {
            var model = GenericModelFactory.Create<InputNormalizationModel>(inputNormalizationType);
            
            var agentAsset = UtilityIntelligenceEditorUtils.Asset;
            if(agentAsset != null)
                agentAsset.InputNormalizations.Add(model);
            return model;
        }

        public void AddInputValueCache(Type inputValueType)
        {
            if (!normalizationCache.ContainsKey(inputValueType))
                normalizationCache.Add(inputValueType, new Dictionary<Type, NormalizationViewModel>());
        }

        public void AddNormalizationToCache(NormalizationViewModel normalization)
        {
            Type normalizationType = normalization.RuntimeType;
            Type inputValueType = normalization.ValueType;
            AddInputValueCache(inputValueType);
            Dictionary<Type, NormalizationViewModel> typeCache = normalizationCache[inputValueType];
            typeCache[normalizationType] = normalization;
        }

        protected override void OnModelChanged(ConsiderationModel newModel)
        {
            if(newModel == null)
                return;
            
            if (newModel.InputNormalization != null)
                CurrentNormalization = CreateViewModel(newModel.InputNormalization);
            else
                CurrentNormalization = null;
        }
    }
}