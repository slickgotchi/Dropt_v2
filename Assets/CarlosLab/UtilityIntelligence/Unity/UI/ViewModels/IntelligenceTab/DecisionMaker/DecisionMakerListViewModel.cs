#region

using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        DecisionMakerListViewModel : NameListViewModel<UtilityIntelligenceModel, DecisionMakerModel, DecisionMakerViewModel>
    {
        public DecisionMakerListViewModel(IDataAsset asset, UtilityIntelligenceModel model) : base(asset, model)
        {
        }

        public override IReadOnlyList<DecisionMakerModel> ItemModels => Model?.DecisionMakers;


        public override bool Contains(string name)
        {
            return Model.HasDecisionMaker(name);
        }

        protected override bool TryAddModelWithoutRecord(DecisionMakerModel model, int index)
        {
            return Model.TryAddDecisionMaker(index, model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(DecisionMakerModel model)
        {
            return Model.TryRemoveDecisionMaker(model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(string name, DecisionMakerModel model)
        {
            return Model.TryRemoveDecisionMaker(name, model);
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveDecisionMaker(sourceIndex, destIndex);
        }
    }
}