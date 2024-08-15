﻿#region

using System;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionItemViewModelIntelligenceTab : BaseItemViewModel<DecisionModel, DecisionListViewModelIntelligenceTab>, INameViewModel, IStatusViewModel
        , INotifyBindablePropertyChanged
    {
        protected override void OnInit(DecisionModel model)
        {
            decisionContextViewModel.Init(model);
            
            var decisionViewModel = RootViewModel.DecisionListViewModel.GetItemByName(Name);
            this.decisionViewModel = decisionViewModel;
            RegisterDecisionViewModelEvents(decisionViewModel);

            actionListViewModel.ListViewModel = decisionViewModel?.ActionListViewModel;
            actionListViewModel.Init(model, this);
            
            considerationListViewModel.ListViewModelDecisionTab = decisionViewModel?.ConsiderationListViewModel;
            considerationListViewModel.Init(model, this);
        }

        protected override void OnDeinit()
        {
            decisionContextViewModel.Deinit();
            UnregisterDecisionViewModelEvents(decisionViewModel);
            decisionViewModel = null;

            actionListViewModel.ListViewModel = null;
            actionListViewModel.Deinit();
            
            considerationListViewModel.ListViewModelDecisionTab = null;
            considerationListViewModel.Deinit();
        }

        protected override void OnRootViewModelChanged(UtilityIntelligenceViewModel rootViewModel)
        {
            decisionContextViewModel.RootViewModel = rootViewModel;
            actionListViewModel.RootViewModel = rootViewModel;
            considerationListViewModel.RootViewModel = rootViewModel;
        }

        protected override void OnItemAdded()
        {
            decisionMakerViewModel = ListViewModel.DecisionMakerViewModel;
            decisionContextViewModel.DecisionMakerContextViewModel = decisionMakerViewModel?.ContextViewModel;
        }
        
        protected override void OnItemRemoved()
        {
            decisionMakerViewModel = null;
            decisionContextViewModel.DecisionMakerContextViewModel = null;
        }

        #region Context

        public DecisionMakerItemViewModel DecisionMakerViewModel => decisionMakerViewModel;

        
        private DecisionContextViewModel decisionContextViewModel = new();
        
        public DecisionContextViewModel DecisionContextViewModel => decisionContextViewModel;
        
        // public bool IsMatchContext
        // {
        //     get
        //     {
        //         var context = Model.Runtime.Context;
        //         var decision = Model.Runtime;
        //         var decisionMaker = decisionMakerViewModel.Model.Runtime;
        //
        //         if (context.DecisionMaker == decisionMaker && context.Decision == decision)
        //             return true;
        //
        //         return false;
        //     }
        // }

        #endregion

        #region ViewModels

        private DecisionMakerItemViewModel decisionMakerViewModel;

        
        private ActionListViewModelIntelligenceTab actionListViewModel = new();

        public ActionListViewModelIntelligenceTab ActionListViewModel => actionListViewModel;
        
        private ConsiderationListViewModelIntelligenceTab considerationListViewModel = new();

        public ConsiderationListViewModelIntelligenceTab ConsiderationListViewModel => considerationListViewModel;

        public TargetFilterListViewModelDecisionTab TargetFilterListViewModel => decisionViewModel.TargetFilterListViewModel;
        
        #endregion

        #region DecisionItemViewModel

        private DecisionItemViewModel decisionViewModel;

        public DecisionItemViewModel DecisionViewModel => decisionViewModel;
        
        private void RegisterDecisionViewModelEvents(DecisionItemViewModel viewModel)
        {
            viewModel.propertyChanged += OnDecisionViewModel_OnPropertyChanged;
        }

        private void UnregisterDecisionViewModelEvents(DecisionItemViewModel viewModel)
        {
            viewModel.propertyChanged -= OnDecisionViewModel_OnPropertyChanged;
        }
        
        private void OnDecisionViewModel_OnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(e.propertyName);
        }

        #endregion
        
        #region Status

        public Status CurrentStatus
        {
            get
            {
                if (decisionContextViewModel.IsMatchContext)
                    return Model.Runtime.CurrentStatus;
                
                // Debug.Log($"MatchDecisionContext Name: {Model.Runtime.Intelligence.Name} DecisionMaker: {decisionMakerViewModel.Name} Decision: {Name} IsWinner: {decisionContextViewModel.IsWinner}  CurrentStatus: {Model.Runtime.CurrentStatus}");

                return Status.Start;
            }
        }

        public event Action<Status> StatusChanged;

        private void OnStatusChanged(Status newStatus)
        {
            if(decisionContextViewModel.IsMatchContext)
                StatusChanged?.Invoke(newStatus);
            else
                StatusChanged?.Invoke(Status.Start);
        }

        #endregion

        #region Actions

        [CreateProperty] public bool KeepRunningUntilFinished => Model.KeepRunningUntilFinished;

        [CreateProperty] public int MaxRepeatCount => Model.MaxRepeatCount;

        [CreateProperty]
        public int CurrentRepeatCount
        {
            get
            {
                if (decisionContextViewModel.IsMatchContext)
                    return Model.Runtime.Task.CurrentRepeatCount;

                return 0;
            }
        }

        [CreateProperty] public ActionExecutionMode ActionExecutionMode => Model.ActionExecutionMode;
        
        private void OnCurrentRepeatCountChanged()
        {
            if (decisionContextViewModel.IsMatchContext)
                Notify(nameof(CurrentRepeatCount));
        }

        #endregion

        #region Targets

        [CreateProperty] public bool HasNoTarget => Model.HasNoTarget;

        #endregion
        
        #region Decision

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set => Model.Name = value;
        }
        
        [CreateProperty]
        public float Weight
        {
            get => Model.Weight;
            set => Model.Weight = value;
        }

        #endregion

        #region Model

        protected override void OnRegisterModelEvents(DecisionModel model)
        {
            var decision = model.Runtime;
            if (IsRuntime)
            {
                decision.StatusChanged += OnStatusChanged;
                
                if(decision.Task != null) decision.Task.CurrentRepeatCountChanged += OnCurrentRepeatCountChanged;
            }
        }

        protected override void OnUnregisterModelEvents(DecisionModel model)
        {
            var decision = model.Runtime;

            decision.StatusChanged -= OnStatusChanged;
            
            if(decision.Task != null) decision.Task.CurrentRepeatCountChanged -= OnCurrentRepeatCountChanged;
        }
        
        protected override void OnModelChanged(DecisionModel newModel)
        {
            DecisionContextViewModel.Model = newModel;

            ActionListViewModel.Model = newModel;
            ConsiderationListViewModel.Model = newModel;

            Notify(nameof(Name));
            Notify(nameof(ActionExecutionMode));
            Notify(nameof(HasNoTarget));
            Notify(nameof(KeepRunningUntilFinished));
            Notify(nameof(MaxRepeatCount));
        }

        #endregion
        
    }
}