#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        DecisionMakerListViewModel : ContainerViewModel<DecisionMakerContainerModel, DecisionMakerModel, DecisionMakerItemViewModel>
    {
        
        protected override DecisionMakerModel CreateModel(Type runtimeType)
        {
            var model = base.CreateModel(runtimeType);
            
            var intelligenceModel = RootViewModel.Model;
            model.DecisionContainer = intelligenceModel.Decisions;

            return model;
        }
    }
}