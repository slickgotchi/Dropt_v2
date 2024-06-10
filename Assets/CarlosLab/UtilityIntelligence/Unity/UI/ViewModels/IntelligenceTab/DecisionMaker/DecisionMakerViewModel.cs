#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerViewModel : BaseItemViewModel<DecisionMakerModel>, INameViewModel, IScoreViewModel,
        IStatusViewModel, IWinnerViewModel, INotifyBindablePropertyChanged
    {
        private DecisionListViewModel decisions;
        
        public event Action<Status> StatusChanged;

        public DecisionMakerViewModel(IDataAsset asset, DecisionMakerModel model) : base(asset, model)
        {
        }

        public Status CurrentStatus => Model.Runtime.CurrentStatus;

        public DecisionListViewModel Decisions
        {
            get
            {
                if (decisions == null)
                    decisions = ViewModelFactory<DecisionListViewModel>.Create(Asset, Model);
                return decisions;
            }
        }

        protected override void RegisterModelEvents(DecisionMakerModel model)
        {
            if (Asset.IsRuntimeAsset)
                model.Runtime.StatusChanged += OnStatusChanged;
            else
                model.Runtime.ActiveChanged += OnActiveChanged;

            model.Runtime.ScoreChanged += OnScoreChanged;

            model.Runtime.BeforeChangeState += OnBeforeChangeDecision;
            model.Runtime.AfterChangeState += OnAfterChangeDecision;
        }

        protected override void UnregisterModelEvents(DecisionMakerModel model)
        {
            model.Runtime.StatusChanged -= OnStatusChanged;
            model.Runtime.ActiveChanged -= OnActiveChanged;
            model.Runtime.ScoreChanged -= OnScoreChanged;

            model.Runtime.BeforeChangeState -= OnBeforeChangeDecision;
            model.Runtime.AfterChangeState -= OnAfterChangeDecision;
        }

        protected override void OnModelChanged(DecisionMakerModel newModel)
        {
            Decisions.Model = newModel;

            Notify(nameof(Name));
        }

        #region Binding Properties

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name == value)
                    return;

                Record($"DecisionMakerViewModel Name Changed: {value}",
                    () => { Model.Name = value; });
                Notify();
            }
        }

        [CreateProperty] public float Score => Model.Runtime.Score;

        [CreateProperty] public bool IsWinner => Model.Runtime.IsActive;

        [CreateProperty] public string BestDecisionName => Model.Runtime.CurrentState?.Name ?? "None";

        #endregion

        #region Event Handlers

        private void OnStatusChanged(Status newStatus)
        {
            StatusChanged?.Invoke(newStatus);
        }

        private void OnActiveChanged(bool isActive)
        {
            Notify(nameof(IsWinner));
        }

        private void OnScoreChanged()
        {
            Notify(nameof(Score));
        }

        private void OnBeforeChangeDecision(Decision oldDecision)
        {
            if (oldDecision != null) oldDecision.NameChanged -= OnDecisionNameChanged;
        }

        private void OnAfterChangeDecision(Decision newDecision)
        {
            if (newDecision != null) newDecision.NameChanged += OnDecisionNameChanged;

            Notify(nameof(BestDecisionName));
        }

        private void OnDecisionNameChanged()
        {
            Notify(nameof(BestDecisionName));
        }

        #endregion

    }
}