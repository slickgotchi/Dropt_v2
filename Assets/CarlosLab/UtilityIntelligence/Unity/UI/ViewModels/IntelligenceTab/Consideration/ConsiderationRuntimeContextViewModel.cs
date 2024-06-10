#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationRuntimeContextViewModel : ViewModel<ConsiderationModel>, IScoreViewModel, ITargetViewModel
        , IDataSourceViewHashProvider
    {
        private ConsiderationListViewModel considerationListViewModel;
        private ConsiderationViewModel considerationViewModel;
        private DecisionContext decisionContext;

        public ConsiderationRuntimeContextViewModel(IDataAsset asset, ConsiderationModel model) : base(
            asset, model)
        {
        }

        public Type InputValueType => Model.Runtime.Input?.ValueType;
        
        public object DefaultInputValue
        {
            get
            {
                var valueType = InputValueType;
                object defaultValue = valueType.IsClass ? null : Activator.CreateInstance(valueType);
                return defaultValue;
            }
        }

        public ConsiderationContext GetConsiderationContext(string name)
        {
            return decisionContext.GetConsideration(name);
        }

        protected override void OnModelChanged(ConsiderationModel newModel)
        {
            decisionContext = DecisionContext.Null;
        }

        #region ViewModel Properties

        public ConsiderationListViewModel ConsiderationListViewModel
        {
            get => considerationListViewModel;
            set
            {
                if (considerationListViewModel == value)
                    return;

                if (considerationListViewModel != null)
                {
                    considerationListViewModel.ContextChanged -= OnContextChanged;
                }

                considerationListViewModel = value;

                if (considerationListViewModel != null)
                {

                    OnContextChanged(considerationListViewModel.Context);
                    considerationListViewModel.ContextChanged += OnContextChanged;
                }
            }
        }

        public ConsiderationViewModel ConsiderationViewModel
        {
            get => considerationViewModel;
            set => considerationViewModel = value;
        }

        #endregion

        #region Context Properties

        [CreateProperty]
        public object RawInput
        {
            get
            {
                ConsiderationContext context = GetConsiderationContext(Model.Name);
                if (context.Input.RawInput != null)
                    return context.Input.RawInput;
                else
                    return DefaultInputValue;
            }
            set => considerationViewModel.EditorViewModel.Input.ValueObject = value;
        }

        [CreateProperty]
        public float NormalizedInput
        {
            get
            {
                ConsiderationContext context = GetConsiderationContext(Model.Name);
                if (context != ConsiderationContext.Null)
                    return context.Input.NormalizedInput;
                else
                    return 0.0f;
            }
        }

        [CreateProperty]
        public float Score
        {
            get
            {
                ConsiderationContext context = GetConsiderationContext(Model.Name);
                if (context != ConsiderationContext.Null)
                {
                    return context.Score;
                }
                else
                {
                    var responseCurve = ResponseCurve;
                    return InfluenceCurveUtils.Evaluate(0.0f, in responseCurve);
                }
            }
        }
        
        [CreateProperty]
        public float FinalScore
        {
            get
            {
                ConsiderationContext context = GetConsiderationContext(Model.Name);
                if (context != ConsiderationContext.Null)
                {
                    return context.FinalScore;
                }
                else
                {
                    var responseCurve = ResponseCurve;
                    return InfluenceCurveUtils.Evaluate(0.0f, in responseCurve);
                }
            }
        }

        [CreateProperty] public string TargetName => decisionContext.Target?.Name ?? "None";

        [CreateProperty] public InfluenceCurve ResponseCurve => Model.Runtime.ResponseCurve;

        public ConsiderationStatus CurrentStatus
        {
            get
            {
                ConsiderationContext context = GetConsiderationContext(Model.Name);
                if (context != ConsiderationContext.Null)
                    return context.CurrentStatus;
                else
                    return ConsiderationStatus.Discarded;
            }
        }


        #endregion

        #region Event Handlers

        private void OnContextChanged(DecisionContext context)
        {
            decisionContext = context;
            UpdateViewHashCode();
            
            Notify(nameof(CurrentStatus));
        }

        #endregion
    }
}