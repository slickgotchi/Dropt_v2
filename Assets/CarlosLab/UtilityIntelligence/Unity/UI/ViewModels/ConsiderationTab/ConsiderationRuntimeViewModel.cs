#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationRuntimeViewModel : ViewModel<ConsiderationModel>
        , IDataSourceViewHashProvider
    {
        public ConsiderationRuntimeViewModel(IDataAsset asset, ConsiderationModel model) : base(asset,
            model)
        {
        }
        
        public ConsiderationEditorViewModel EditorViewModel;
        public Type InputValueType => Model.Runtime.Input?.ValueType;

        protected override void OnRegisterModelEvents(ConsiderationModel model)
        {
            model.Runtime.ScoreChanged += OnScoreChanged;
        }

        protected override void OnUnregisterModelEvents(ConsiderationModel model)
        {
            model.Runtime.ScoreChanged -= OnScoreChanged;
        }

        #region Binding Properties

        [CreateProperty]
        public InfluenceCurve ResponseCurve
        {
            get => Model.Runtime.ResponseCurve;
            set
            {
                Model.ResponseCurve = value;
                OnResponCurveChanged();
            }
        }


        [CreateProperty]
        public object RawInput
        {
            get => Model.Runtime.RawInput;
            set => EditorViewModel.RawInput = value;
        }

        [CreateProperty] public float NormalizedInput => Model.Runtime.NormalizedInput;

        [CreateProperty] public float Score => Model.Runtime.Score;

        #endregion

        #region Event Handlers

        private void OnResponCurveChanged()
        {
            EditorViewModel?.CalculateScore();

            UpdateViewHashCode();
        }

        private void OnScoreChanged()
        {
            UpdateViewHashCode();
        }

        #endregion
    }
}