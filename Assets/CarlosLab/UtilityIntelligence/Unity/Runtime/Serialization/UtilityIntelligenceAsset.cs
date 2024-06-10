#region

using System.Collections.Generic;
using System.Reflection;
using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [CreateAssetMenu(menuName = FrameworkRuntimeConsts.CreateAssetMenuPath, fileName = FrameworkRuntimeConsts.AssetFileName)]
    public class UtilityIntelligenceAsset : DataAsset<UtilityIntelligenceModel, UtilityIntelligence>
    {
        [SerializeField]
        internal string agentType;

        [SerializeField]
        internal string agentDescription;

        #region Framework Properties

        public override int DataVersion => FrameworkRuntimeConsts.DataVersion;
        public override string FrameworkVersion => FrameworkRuntimeConsts.FrameworkVersion;

        #endregion

        #region Agent Properties

        public List<DecisionModel> Decisions { get; } = new();
        public List<ActionModel> Actions { get; } = new();
        public List<TargetFilterModel> TargetFilters { get; } = new();
        public List<ConsiderationModel> Considerations { get; } = new();
        public List<InputModel> Inputs { get; } = new();
        public List<InputNormalizationModel> InputNormalizations { get; } = new();

        #endregion

        #region Serialization Methods

        protected override void OnStartDeserialization()
        {
            ResetData();
        }
        
        protected override void OnFinishDeserialization()
        {
            UpdateReferences();
        }

        #endregion

        #region Update Model Methods
        
        private void ResetData()
        {
            Decisions.Clear();
            TargetFilters.Clear();
            Actions.Clear();

            Considerations.Clear();
            Inputs.Clear();
            InputNormalizations.Clear();
        }

        private void UpdateReferences()
        {
            if (Model == null) return;
            
            TargetFilterContainerModel targetFilters = Model.TargetFilters;
            foreach (DecisionModel model in Decisions)
            {
                model.TargetFilterContainer = targetFilters;
            }
            
            ConsiderationContainerModel considerations = Model.Considerations;
            foreach (DecisionModel model in Decisions)
            {
                model.ConsiderationContainer = considerations;
            }

            InputContainerModel inputs = Model.Inputs;

            foreach (ConsiderationModel model in Considerations)
            {
                model.InputContainer = inputs;
            }

            UpdateVariableReferences(TargetFilters);
            UpdateVariableReferences(Actions);
            UpdateVariableReferences(Inputs);
            UpdateVariableReferences(InputNormalizations);
        }

        private void UpdateVariableReferences(IReadOnlyList<IGenericModel> models)
        {
            Blackboard blackboard = Model?.Blackboard.Runtime;
            if (blackboard == null) return;

            using var _ = Profiler.Sample("UtilityAgentAsset - UpdateVariableReferences");

            for (int modelIndex = 0; modelIndex < models.Count; modelIndex++)
            {
                IGenericModel model = models[modelIndex];
                var variableReferenceFields = model.VariableReferenceFields;
                
                for (int variableIndex = 0; variableIndex < variableReferenceFields.Count; variableIndex++)
                {
                    FieldInfo fieldInfo = variableReferenceFields[variableIndex];
                    object fieldValue = model.GetValue(fieldInfo.Name);
                    if (fieldValue is IVariableReference variableReference) variableReference.Blackboard = blackboard;
                }
            }
        }

        #endregion
    }
}