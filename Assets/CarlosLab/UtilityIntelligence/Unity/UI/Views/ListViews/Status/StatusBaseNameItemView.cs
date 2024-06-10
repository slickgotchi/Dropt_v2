#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class StatusBaseNameItemView<TItemViewModel> : BaseNameItemView<TItemViewModel>
        where TItemViewModel : BaseItemViewModel, IStatusViewModel
    {
        private const string RunningClassName = "running";
        private const string FailureClassName = "failure";
        private const string AbortedClassName = "aborted";
        private const string SuccessClassName = "success";

        private readonly bool enableStatus;

        protected StatusBaseNameItemView(bool enableStatus, bool enableRename,
            IListViewWithItem<TItemViewModel> listView) : base(enableRename, listView)
        {
            this.enableStatus = enableStatus;
        }
        protected override void OnRegisterViewModelEvents(TItemViewModel viewModel)
        {
            if (enableStatus)
            {
                UpdateStatus(viewModel.CurrentStatus, true);
                viewModel.StatusChanged += OnStatusChanged;
            }
        }

        protected override void OnUnregisterViewModelEvents(TItemViewModel viewModel)
        {
            if (enableStatus)
            {
                viewModel.StatusChanged -= OnStatusChanged;
                // UpdateStatus(Status.Start, true);
            }
        }
        
        // protected override void OnEnteredEditMode()
        // {
        //     if (enableStatus)
        //     {
        //         ViewModel.StatusChanged -= OnStatusChanged;
        //         OnStatusChanged(Status.Start);
        //     }
        // }
        
        private void EnableStatus(Status status)
        {
            // if (ViewModel.ModelObject is ConsiderationModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"Agent: {runtime.Agent.Name} Consideration: {runtime.Name} ItemView EnableStatus: {status}");
            // }
            switch (status)
            {
                case Status.Running:
                    EnableStateClass(RunningClassName);
                    break;
                case Status.Success:
                    EnableStateClass(SuccessClassName);
                    break;
                case Status.Failure:
                    EnableStateClass(FailureClassName);
                    break;
                case Status.Aborted:
                    EnableStateClass(AbortedClassName);
                    break;
            }
        }

        private void DisableStatus(Status status, bool immediate)
        {
            // if (ViewModel.ModelObject is ConsiderationModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"Agent: {runtime.Agent.Name} Consideration: {runtime.Name} ItemView DisableStatus: {status}");
            // }
            switch (status)
            {
                case Status.Running:
                    DisableStateClass(RunningClassName);
                    break;
                case Status.Success:
                    if(immediate)
                        DisableStateClass(SuccessClassName);
                    else
                        DisableStateClassAfter(100, SuccessClassName);
                    break;
                case Status.Failure:
                    if(immediate)
                        DisableStateClass(FailureClassName);
                    else
                        DisableStateClassAfter(100, FailureClassName);
                    break;
                case Status.Aborted:
                    if(immediate)
                        DisableStateClass(AbortedClassName);
                    else
                        DisableStateClassAfter(100, AbortedClassName);
                    break;
            }
        }

        protected void EnableStateClass(string stateClassName)
        {
            // if (ViewModel.ModelObject is ConsiderationModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"Agent: {runtime.Agent.Name} Consideration: {runtime.Name} ItemView EnableStateClass: {stateClassName}");
            // }
            ItemContainerView?.AddToClassList(stateClassName);
        }

        protected void DisableStateClass(string stateClassName)
        {
            // if (ViewModel.ModelObject is ConsiderationModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"Agent: {runtime.Agent.Name} Consideration: {runtime.Name} ItemView DisableStateClass: {stateClassName}");
            // }
            ItemContainerView?.RemoveFromClassList(stateClassName);
        }

        protected void DisableStateClassAfter(long delayTime, string stateClassName)
        {
            // if (ViewModel.ModelObject is ConsiderationModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"Agent: {runtime.Agent.Name} Consideration: {runtime.Name} ItemView DisableStateClassAfter: {stateClassName}");
            // }
            schedule.Execute(() => DisableStateClass(stateClassName)).StartingIn(delayTime);
        }

        #region Status

        private Status currentStatus;

        public Status CurrentStatus
        {
            get => currentStatus;
            set => UpdateStatus(value, true);
        }

        private void UpdateStatus(Status newStatus, bool immediate)
        {
            if (currentStatus == newStatus)
                return;

            // if (ViewModel.ModelObject is ConsiderationModel model)
            // {
            //     var runtime = model.Runtime;
            //     Debug.Log($"Agent: {runtime.Agent.Name} Consideration: {runtime.Name} ItemView StatusChanged From: {currentStatus} To: {newStatus}");
            // }

            DisableStatus(currentStatus, immediate);
            currentStatus = newStatus;
            EnableStatus(currentStatus);
        }
        
        private void OnStatusChanged(Status newStatus)
        {
            CurrentStatus = newStatus;
        }

        #endregion
    }
}