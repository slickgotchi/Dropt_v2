using System;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterEditorListViewModel :
        ItemContainerViewModel<TargetFilterContainerModel, TargetFilterModel, TargetFilterEditorViewModel>
    {
        public TargetFilterEditorListViewModel(IDataAsset asset, TargetFilterContainerModel model) :
            base(asset, model)
        {
        }

        protected override TargetFilterModel CreateModel(Type runtimeType)
        {
            var model = GenericModelFactory.CreateWithId<TargetFilterModel>(runtimeType);
            var agentAsset = UtilityIntelligenceEditorUtils.Asset;
            if(agentAsset != null)
                agentAsset.TargetFilters.Add(model);
            return model;
        }
    }
}