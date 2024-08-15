#region

using System;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerItemViewModel : BaseItemViewModel<DecisionMakerModel, DecisionMakerListViewModel>, INameViewModel,
        IStatusViewModel, INotifyBindablePropertyChanged
    {
        protected override void OnInit(DecisionMakerModel model)
        {
            contextViewModel.Init(model);
            decisionListViewModel.Init(model, this);
        }

        protected override void OnDeinit()
        {
            contextViewModel.Deinit();
            decisionListViewModel.Deinit();
        }

        protected override void OnRootViewModelChanged(UtilityIntelligenceViewModel rootViewModel)
        {
            contextViewModel.RootViewModel = RootViewModel;
            decisionListViewModel.RootViewModel = rootViewModel;
        }

        #region ViewModels

        private DecisionMakerContextViewModel contextViewModel = new();

        public DecisionMakerContextViewModel ContextViewModel => contextViewModel;
        
        private DecisionListViewModelIntelligenceTab decisionListViewModel = new();
        public DecisionListViewModelIntelligenceTab DecisionListViewModel => decisionListViewModel;



        #endregion

        #region Status

        public Status CurrentStatus => Model.Runtime.CurrentStatus;
        public event Action<Status> StatusChanged;
        
        private void OnStatusChanged(Status newStatus)
        {
            StatusChanged?.Invoke(newStatus);
        }

        #endregion

        #region Model

        protected override void OnRegisterModelEvents(DecisionMakerModel model)
        {
            if (IsRuntime)
                model.Runtime.StatusChanged += OnStatusChanged;
        }

        protected override void OnUnregisterModelEvents(DecisionMakerModel model)
        {
            model.Runtime.StatusChanged -= OnStatusChanged;
        }

        protected override void OnModelChanged(DecisionMakerModel newModel)
        {
            ContextViewModel.Model = newModel;
            DecisionListViewModel.Model = newModel;

            if (newModel == null) return;
            
            Notify(nameof(Name));
        }

        #endregion

        #region DecisionMaker

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

        #endregion

    }
}