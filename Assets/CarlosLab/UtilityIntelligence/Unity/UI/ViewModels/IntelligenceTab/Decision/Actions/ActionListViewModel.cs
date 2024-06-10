#region

using System;
using System.Collections.Generic;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionListViewModel : BaseListViewModel<DecisionModel, ActionModel, ActionViewModel>
    {
        public ActionListViewModel(IDataAsset asset, DecisionModel model) : base(asset, model)
        {
        }

        public DecisionRuntimeContextViewModel ContextViewModel { get; set; }

        public override IReadOnlyList<ActionModel> ItemModels => Model.Actions;

        protected override ActionModel CreateModel(Type runtimeType)
        {
            var model = GenericModelFactory.CreateWithId<ActionModel>(runtimeType);
            var agentAsset = UtilityIntelligenceEditorUtils.Asset;
            
            if(agentAsset != null)
                agentAsset.Actions.Add(model);
            return model;
        }

        protected override bool TryAddModelWithoutRecord(ActionModel model, int index)
        {
            Model.AddAction(index, model, Asset.IsRuntimeAsset);
            return true;
        }

        protected override bool TryRemoveModelWithoutRecord(ActionModel model)
        {
            Model.RemoveAction(model);
            return true;
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveAction(sourceIndex, destIndex);
        }
    }
}