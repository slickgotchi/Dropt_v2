#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionListViewModel : NameListViewModel<DecisionMakerModel, DecisionModel, DecisionViewModel>
    {
        
        public DecisionListViewModel(IDataAsset asset, DecisionMakerModel model) : base(asset, model)
        {
        }

        public override IReadOnlyList<DecisionModel> ItemModels => Model.Decisions;

        protected override DecisionModel CreateModel(Type runtimeType)
        {
            DecisionModel model = base.CreateModel(runtimeType);

            var agentModel = UtilityIntelligenceEditorUtils.Model;
            if (agentModel != null)
            {
                model.TargetFilterContainer = agentModel.TargetFilters;
                model.ConsiderationContainer = agentModel.Considerations;
            }

            var agentAsset = UtilityIntelligenceEditorUtils.Asset;
            if(agentAsset != null)
                agentAsset.Decisions.Add(model);
            return model;
        }

        public override bool Contains(string name)
        {
            return Model.HasDecision(name);
        }

        protected override bool TryAddModelWithoutRecord(DecisionModel model, int index)
        {
            return Model.TryAddDecision(index, model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(DecisionModel model)
        {
            return Model.TryRemoveDecision(model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(string name, DecisionModel model)
        {
            return Model.TryRemoveDecision(name, model);
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveDecision(sourceIndex, destIndex);
        }
    }
}