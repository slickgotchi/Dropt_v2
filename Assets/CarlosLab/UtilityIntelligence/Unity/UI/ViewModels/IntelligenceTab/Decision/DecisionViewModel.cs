#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionViewModel : BaseItemViewModel<DecisionModel>, INameViewModel, IStatusViewModel
        , IWinnerViewModel, INotifyBindablePropertyChanged
    {
        #region Fields

        private ActionListViewModel actions;
        private ConsiderationListViewModel considerations;

        private DecisionRuntimeContextViewModel contextViewModel;
        private TargetFilterListViewModel targetFilters;

        #endregion
        
        public event Action<Status> StatusChanged;


        public DecisionViewModel(IDataAsset asset, DecisionModel model) : base(asset, model)
        {
        }

        #region Normal Properties
        
        public Status CurrentStatus => Model.Runtime.CurrentStatus;

        [CreateProperty]
        public bool KeepRunningUntilFinished
        {
            get => Model.KeepRunningUntilFinished;
            set
            {
                if (Model.KeepRunningUntilFinished == value) return;
                
                Record($"DecisionViewModel KeepRunningUntilFinished Changed: {value}",
                    () => { Model.KeepRunningUntilFinished = value; });
            }
        }
        
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
        
        [CreateProperty]
        public int MaxRepeatCount
        {
            get => Model.MaxRepeatCount;
            set
            {
                if (Model.MaxRepeatCount == value) return;
                
                Record($"DecisionViewModel MaxRepeatCount Changed: {value}",
                    () => { Model.MaxRepeatCount = value; });
            }
        }
        

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

                Record($"DecisionViewModel Name Changed: {value}",
                    () => { Model.Name = value; });
                Notify();
            }
        }

        [CreateProperty]
        public ActionExecutionMode ActionExecutionMode
        {
            get => Model.ActionExecutionMode;
            set
            {
                if (Model.ActionExecutionMode == value)
                    return;

                Record($"DecisionViewModel ActionsExecutionMode Changed: {value}",
                    () =>
                    {
                        Model.SetActionsExecutionMode(value, Asset.IsRuntimeAsset);
                    });

                Notify();
            }
        }

        [CreateProperty] public bool IsWinner => Model.Runtime.IsActive;
        
        [CreateProperty]
        public int CurrentRepeatCount => Model.CurrentRepeatCount;

        #endregion
        
        #region ViewModel Properties

        public ActionListViewModel Actions
        {
            get
            {
                if (actions == null)
                {
                    actions = ViewModelFactory<ActionListViewModel>.Create(Asset, Model);
                    actions.ContextViewModel = ContextViewModel;
                }

                return actions;
            }
        }

        public TargetFilterListViewModel TargetFilters
        {
            get
            {
                if (targetFilters == null)
                {
                    targetFilters = ViewModelFactory<TargetFilterListViewModel>.Create(Asset, Model);
                }

                return targetFilters;
            }
        }

        public ConsiderationListViewModel Considerations
        {
            get
            {
                if (considerations == null)
                    considerations = ViewModelFactory<ConsiderationListViewModel>.Create(Asset, Model);

                return considerations;
            }
        }

        public DecisionRuntimeContextViewModel ContextViewModel
        {
            get
            {
                if (contextViewModel == null)
                    contextViewModel = ViewModelFactory<DecisionRuntimeContextViewModel>.Create(Asset, Model);

                return contextViewModel;
            }
        }

        #endregion

        #region Register/Unregister Model Events

        protected override void RegisterModelEvents(DecisionModel model)
        {
            var decision = model.Runtime;
            if (Asset.IsRuntimeAsset)
            {
                decision.StatusChanged += OnStatusChanged;
                
                if(decision.Task != null) decision.Task.CurrentRepeatCountChanged += OnCurrentRepeatCountChanged;
            }
            else
            {
                decision.ActiveChanged += OnActiveChanged;
            }
        }

        protected override void UnregisterModelEvents(DecisionModel model)
        {
            var decision = model.Runtime;

            decision.StatusChanged -= OnStatusChanged;
            decision.ActiveChanged -= OnActiveChanged;
            
            if(decision.Task != null) decision.Task.CurrentRepeatCountChanged -= OnCurrentRepeatCountChanged;
        }

        #endregion

        #region Event Functions
        
        protected override void OnModelChanged(DecisionModel newModel)
        {
            Actions.Model = newModel;
            TargetFilters.Model = newModel;
            Considerations.Model = newModel;
            ContextViewModel.Model = newModel;

            Notify(nameof(Name));
            Notify(nameof(ActionExecutionMode));
            Notify(nameof(HasNoTarget));
            Notify(nameof(KeepRunningUntilFinished));
            Notify(nameof(MaxRepeatCount));
        }
        
        private void OnCurrentRepeatCountChanged()
        {
            Notify(nameof(CurrentRepeatCount));
        }

        private void OnStatusChanged(Status newStatus)
        {
            StatusChanged?.Invoke(newStatus);
        }

        private void OnActiveChanged(bool isActive)
        {
            Notify(nameof(IsWinner));
        }

        #endregion

    }
}