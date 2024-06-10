#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionRuntimeContextViewModel : ViewModel<DecisionModel>, IScoreViewModel, ITargetViewModel
        , IDataSourceViewHashProvider
    {
        private DecisionContext decisionContext;

        public DecisionRuntimeContextViewModel(IDataAsset asset, DecisionModel model) : base(asset,
            model)
        {
        }

        protected override void OnRegisterModelEvents(DecisionModel model)
        {
            decisionContext = model.Runtime.Context;
            model.Runtime.ContextChanged += OnContextChanged;
        }

        protected override void OnUnregisterModelEvents(DecisionModel model)
        {
            model.Runtime.ContextChanged -= OnContextChanged;
        }

        protected override void OnModelChanged(DecisionModel newModel)
        {
            decisionContext = DecisionContext.Null;
        }

        #region Binding Properties

        [CreateProperty] public string TargetName => decisionContext.Target?.Name ?? "None";

        [CreateProperty]
        public float Score
        {
            get
            {
                if (decisionContext != DecisionContext.Null)
                    return decisionContext.Score;
                else
                    return 0.0f;
            }
        }

        [CreateProperty]
        public float Weight
        {
            get => Model.Weight;
            set
            {
                if (Math.Abs(Model.Weight - value) < MathUtils.Epsilon)
                    return;

                Record($"DecisionContextViewModel Weight Changed: {value}",
                    () => { Model.Weight = value; });

                Notify();
            }
        }

        #endregion

        #region Event Handlers

        private void OnContextChanged(DecisionContext context)
        {
            decisionContext = context;
            UpdateViewHashCode();
        }

        #endregion
    }
}