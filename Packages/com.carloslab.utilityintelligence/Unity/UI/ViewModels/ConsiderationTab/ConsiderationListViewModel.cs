#region

using System;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationListViewModel :
        ContainerViewModel<ConsiderationContainerModel, ConsiderationModel, ConsiderationItemViewModel>
    {
        protected override ConsiderationModel CreateModel(Type runtimeType)
        {
            ConsiderationModel model = base.CreateModel(runtimeType);
            
            var intelligenceModel = RootViewModel.Model;
            model.InputNormalizationContainer = intelligenceModel.InputNormalizations;

            return model;
        }
    }
}