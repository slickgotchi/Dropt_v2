#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationEditorListViewModel :
        ItemContainerViewModel<ConsiderationContainerModel, ConsiderationModel, ConsiderationEditorViewModel>
    {
        public ConsiderationEditorListViewModel(IDataAsset asset, ConsiderationContainerModel model) :
            base(asset, model)
        {
        }

        protected override ConsiderationModel CreateModel(Type runtimeType)
        {
            ConsiderationModel model = base.CreateModel(runtimeType);
            
            var intelligenceModel = UtilityIntelligenceEditorUtils.Model;
            if(intelligenceModel != null)
                model.InputContainer = intelligenceModel.Inputs;
            
            var intelligenceAsset = UtilityIntelligenceEditorUtils.Asset;
            if(intelligenceAsset != null)
                intelligenceAsset.Considerations.Add(model);
            return model;
        }
    }
}