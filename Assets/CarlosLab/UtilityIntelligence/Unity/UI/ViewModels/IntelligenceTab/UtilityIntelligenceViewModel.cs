#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class UtilityIntelligenceViewModel : ViewModel<UtilityIntelligenceModel>, IRootViewModel, INameViewModel
    {

        public UtilityIntelligenceViewModel(IDataAsset asset, UtilityIntelligenceModel model) : base(asset, model)
        {
        }

        #region Name

        private string name = "None";
        [CreateProperty]
        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;

                name = value;
                Notify();
            }
        }

        #endregion

        #region Data Asset

        public int DataVersion => Model?.DataVersion ?? 0;

        public bool IsEditorOpening
        {
            
            get => Model?.IsEditorOpening ?? false;
            set
            {
                if (Model != null) Model.IsEditorOpening = value;
            }
        }

        public bool IsRuntimeAsset
        {
            get => Model?.IsEditorOpening ?? false;
            set
            {
                if (Model != null) Model.IsRuntimeAsset = value;
            }
        }

        #endregion

        #region Target Filters

        private TargetFilterEditorListViewModel targetFilters;

        public TargetFilterEditorListViewModel TargetFilters
        {
            get
            {
                if (targetFilters == null)
                {
                    targetFilters =
                        ViewModelFactory<TargetFilterEditorListViewModel>.Create(Asset, Model?.TargetFilters);
                }

                return targetFilters;
            }
        }

        #endregion

        #region Considerations

        private ConsiderationEditorListViewModel considerations;


        public ConsiderationEditorListViewModel Considerations
        {
            get
            {
                if (considerations == null)
                {
                    considerations =
                        ViewModelFactory<ConsiderationEditorListViewModel>.Create(Asset, Model?.Considerations);
                }

                return considerations;
            }
        }
        
        [CreateProperty]
        public bool EnableCompensationFactor
        {
            
            get => Model?.EnableCompensationFactor ?? false;
            set
            {
                if (Model != null)
                {
                    Record($"UtilityIntelligenceViewModel EnableCompensationFactor Changed: {value}",
                        () => { Model.EnableCompensationFactor = value; });

                    MakeDecision();
                }
            }
        }
        
        [CreateProperty]
        public bool EnableMomentumBonus
        {
            
            get => Model?.EnableMomentumBonus ?? false;
            set
            {
                if (Model != null)
                {
                    Record($"UtilityIntelligenceViewModel EnableMomentumBonus Changed: {value}",
                        () => { Model.EnableMomentumBonus = value; });

                    MakeDecision();
                }
            }
        }

        #endregion

        #region Inputs

        private InputListViewModel inputs;


        public InputListViewModel Inputs
        {
            get
            {
                if (inputs == null)
                    inputs = ViewModelFactory<InputListViewModel>.Create(Asset, Model?.Inputs);

                return inputs;
            }
        }

        #endregion

        #region Blackboard

        private BlackboardViewModel blackboard;


        public BlackboardViewModel Blackboard
        {
            get
            {
                if (blackboard == null)
                    blackboard = ViewModelFactory<BlackboardViewModel>.Create(Asset, Model?.Blackboard);

                return blackboard;
            }
        }

        #endregion

        #region Decision Makers

        private DecisionMakerListViewModel decisionMakers;


        public DecisionMakerListViewModel DecisionMakers
        {
            get
            {
                if (decisionMakers == null)
                    decisionMakers = ViewModelFactory<DecisionMakerListViewModel>.Create(Asset, Model);

                return decisionMakers;
            }
        }

        #endregion
        

        protected override void OnModelChanged(UtilityIntelligenceModel newModel)
        {
            if(newModel == null)
                return;

            Blackboard.Model = newModel.Blackboard;
            Inputs.Model = newModel.Inputs;
            Considerations.Model = newModel.Considerations;
            TargetFilters.Model = newModel.TargetFilters;
            DecisionMakers.Model = newModel;

            Notify(nameof(Name));
            Notify(nameof(EnableCompensationFactor));

            MakeDecision();
        }

        public void MakeDecision()
        {
            var intelligenceModel = UtilityIntelligenceEditorUtils.Model;
            if (!Asset.IsRuntimeAsset && intelligenceModel != null)
                intelligenceModel.Runtime.MakeDecision(null);
        }
    }
}