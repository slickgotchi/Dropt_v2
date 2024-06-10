#region

using System;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputListViewModel : ItemContainerViewModel<InputContainerModel, InputModel, InputItemViewModel>,
        INameListViewModel
    {
        public InputListViewModel(IDataAsset asset, InputContainerModel model) : base(asset, model)
        {
        }

        protected override InputModel CreateModel(Type runtimeType)
        {
            var model = GenericModelFactory.CreateWithId<InputModel>(runtimeType);
            var agentAsset = UtilityIntelligenceEditorUtils.Asset;
            if(agentAsset != null)
                agentAsset.Inputs.Add(model);
            return model;
        }
    }
}