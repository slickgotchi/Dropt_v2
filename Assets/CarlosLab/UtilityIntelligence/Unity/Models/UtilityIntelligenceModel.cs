#region

using System.Collections.Generic;
using System.Runtime.Serialization;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class UtilityIntelligenceModel : Model<UtilityIntelligence>, TRootModel
    {
        #region Data Asset

        public bool IsEditorOpening
        {
            get => Runtime.IsEditorOpening;
            internal set => Runtime.IsEditorOpening = value;
        }
        
        public bool IsRuntimeAsset
        {
            get => Runtime.IsRuntimeAsset;
            internal set => Runtime.IsRuntimeAsset = value;
        }
        
        bool TRootModel.IsEditorOpening
        {
            get => Runtime.IsEditorOpening;
            set => Runtime.IsEditorOpening = value;
        }

        bool TRootModel.IsRuntimeAsset
        {
            get => Runtime.IsRuntimeAsset;
            set => Runtime.IsRuntimeAsset = value;
        }

        #endregion
        
        #region Version

        [DataMember(Name = nameof(DataVersion))]
        private int dataVersion;

        int TRootModel.DataVersion
        {
            get => dataVersion;
            set => dataVersion = value;
        }

        public int DataVersion => dataVersion;

        [DataMember(Name = nameof(FrameworkVersion))]
        private string frameworkVersion;

        string TRootModel.FrameworkVersion
        {
            get => frameworkVersion;
            set => frameworkVersion = value;
        }

        public string FrameworkVersion => frameworkVersion;

        #endregion
        
        #region Blackboard

        [DataMember(Name = nameof(Blackboard))]
        private BlackboardModel blackboard = new();

        public BlackboardModel Blackboard => blackboard;

        #endregion
        
        #region TargetFilters

        [DataMember(Name = nameof(TargetFilters))]
        private TargetFilterContainerModel targetFilters = new();

        public TargetFilterContainerModel TargetFilters => targetFilters;

        #endregion
        
        #region Considerations

        [DataMember(Name = nameof(Considerations))]
        private ConsiderationContainerModel considerations = new();

        public ConsiderationContainerModel Considerations => considerations;

        [DataMember(Name = nameof(EnableCompensationFactor))]
        private bool enableCompensationFactor = true;
        public bool EnableCompensationFactor
        {
            get => enableCompensationFactor;
            set
            {
                enableCompensationFactor = value;
                Runtime.EnableCompensationFactor = enableCompensationFactor;
            }
        }
        
        [DataMember(Name = nameof(EnableMomentumBonus))]
        private bool enableMomentumBonus = true;
        public bool EnableMomentumBonus
        {
            get => enableMomentumBonus;
            set
            {
                enableMomentumBonus = value;
                Runtime.EnableMomentumBonus = enableMomentumBonus;
            }
        }

        #endregion
        
        #region Inputs

        [DataMember(Name = nameof(Inputs))]
        private InputContainerModel inputs = new();

        public InputContainerModel Inputs => inputs;


        #endregion
        
        #region Runtime
        private UtilityIntelligence runtime;

        public override UtilityIntelligence Runtime
        {
            get
            {
                if (runtime == null)
                {
                    runtime = new UtilityIntelligence(targetFilters.Runtime
                        , considerations.Runtime
                        , inputs.Runtime
                        , blackboard.Runtime)
                    {
                        EnableCompensationFactor = enableCompensationFactor,
                        EnableMomentumBonus = enableMomentumBonus
                    };
                    
                    foreach (DecisionMakerModel decisionMaker in decisionMakers)
                    {
                        runtime.TryAddDecisionMaker(decisionMaker.Name, decisionMaker.Runtime);
                    }
                }

                return runtime;
            }
        }

        #endregion

        #region DecisionMakers
        
        [DataMember(Name = nameof(DecisionMakers))]
        private List<DecisionMakerModel> decisionMakers = new();

        public IReadOnlyList<DecisionMakerModel> DecisionMakers => decisionMakers;


        public bool HasDecisionMaker(string name)
        {
            return Runtime.HasDecisionMaker(name);
        }

        public bool TryAddDecisionMaker(int index, string name, DecisionMakerModel decisionMaker)
        {
            if (Runtime.TryAddDecisionMaker(index, name, decisionMaker.Runtime))
            {
                decisionMakers.Insert(index, decisionMaker);
                return true;
            }

            return false;
        }

        public DecisionMakerModel GetDecisionMakerById(string id)
        {
            return decisionMakers.Find(x => x.Id == id);
        }

        public bool TryRemoveDecisionMaker(string name, DecisionMakerModel decisionMaker)
        {
            if (Runtime.TryRemoveDecisionMaker(name))
            {
                decisionMakers.Remove(decisionMaker);
                return true;
            }

            return false;
        }

        public void MoveDecisionMaker(int sourceIndex, int destIndex)
        {
            decisionMakers.Move(sourceIndex, destIndex);
            Runtime.MoveDecisionMaker(sourceIndex, destIndex);
        }

        #endregion
    }
}