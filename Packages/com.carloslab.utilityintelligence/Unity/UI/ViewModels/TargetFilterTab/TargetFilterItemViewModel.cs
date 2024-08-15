using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterItemViewModel : BaseItemViewModel<TargetFilterModel, TargetFilterListViewModel>,
        INameViewModel, ITypeNameViewModel, INotifyBindablePropertyChanged
    {

        #region Properties

        public string TypeName => Model?.RuntimeType.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;

        [CreateProperty]
        public string Name
        {
            get => Model?.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;
            set
            {
                if (Model == null || Model.Name == value)
                    return;

                Record($"TargetFilterItemViewModel Name Changed: {value}",
                    () => { Model.Name = value; });

                Notify();
            }
        }

        #endregion

        #region Model

        protected override void OnModelChanged(TargetFilterModel newModel)
        {
            Notify(nameof(Name));
        }

        #endregion
        
        #region RootViewModel

        protected override void OnRegisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.BlackboardViewModel.ItemRemoved += BlackboardViewModel_OnItemRemoved;
            viewModel.BlackboardViewModel.ItemNameChanged += BlackboardViewModel_OnItemNameChanged;
        }

        protected override void OnUnregisterRootViewModelEvents(UtilityIntelligenceViewModel viewModel)
        {
            viewModel.BlackboardViewModel.ItemRemoved -= BlackboardViewModel_OnItemRemoved;
            viewModel.BlackboardViewModel.ItemNameChanged -= BlackboardViewModel_OnItemNameChanged;

        }

        #endregion
        
        #region BlackboardViewModel

        private void BlackboardViewModel_OnItemRemoved(VariableViewModel variable)
        {
            Model.SetVariableReference(variable.Name, null);
        }
        
        private void BlackboardViewModel_OnItemNameChanged(VariableViewModel variable, string oldName, string newName)
        {
            Model.SetVariableReference(oldName, newName);
        }

        #endregion
    }
}