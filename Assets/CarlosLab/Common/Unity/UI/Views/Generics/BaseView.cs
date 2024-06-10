using System;

namespace CarlosLab.Common.UI
{
    public abstract class BaseView<TViewModel> : BaseView
        where TViewModel : class, IViewModel
    {
        private TViewModel viewModel;

        protected BaseView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        public TViewModel ViewModel
        {
            get => viewModel;
            protected set
            {
                if (viewModel == value)
                    return;

                if (viewModel != null) UnregisterViewModelEvents(viewModel);
                viewModel = value;

                HandleViewModelChanged(viewModel);

                if (viewModel != null) RegisterViewModelEvents(viewModel);
            }
        }

        public event Action<TViewModel> ViewModelChanged;

        protected bool ValidateViewModel(TViewModel viewModel)
        {
            if (viewModel == null)
                return false;

            if (viewModel == ViewModel)
                return false;

            return true;
        }

        public virtual bool UpdateView(TViewModel viewModel)
        {
            if (!ValidateViewModel(viewModel))
                return false;

            ViewModel = viewModel;

            dataSource = ViewModel;

            OnUpdateView(viewModel);

            if (ViewModel.Asset.IsRuntimeAsset)
            {
                OnEnableRuntimeMode();
            }
            else
            {
                OnEnableEditMode();
            }

            return true;
        }
        
        #region Edit/Runtime Mode Functions

        protected virtual void OnEnableEditMode()
        {
        }

        protected virtual void OnEnableRuntimeMode()
        {
        }

        #endregion

        private void HandleViewModelChanged(TViewModel viewModel)
        {
            OnViewModelChanged(viewModel);
            ViewModelChanged?.Invoke(viewModel);
        }

        private void RegisterViewModelEvents(TViewModel viewModel)
        {
            viewModel.ModelChanged += OnModelChanged;
            OnRegisterViewModelEvents(viewModel);
        }

        protected void UnregisterViewModelEvents(TViewModel viewModel)
        {
            viewModel.ModelChanged -= OnModelChanged;
            OnUnregisterViewModelEvents(viewModel);
        }

        protected virtual void OnModelChanged()
        {
        }

        #region Event Functions

        protected virtual void OnUpdateView(TViewModel viewModel)
        {
        }

        protected virtual void OnRegisterViewModelEvents(TViewModel viewModel)
        {
        }

        protected virtual void OnUnregisterViewModelEvents(TViewModel viewModel)
        {
        }

        protected virtual void OnViewModelChanged(TViewModel viewModel)
        {
        }

        #endregion
    }
}