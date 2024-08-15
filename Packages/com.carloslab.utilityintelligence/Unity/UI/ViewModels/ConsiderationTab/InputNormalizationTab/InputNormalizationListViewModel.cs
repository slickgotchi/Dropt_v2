﻿using System;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputNormalizationListViewModel : ContainerViewModel<InputNormalizationContainerModel, InputNormalizationModel, InputNormalizationItemViewModel>
    {
        protected override InputNormalizationModel CreateModel(Type runtimeType)
        {
            var blackboard = Model.Runtime.Blackboard;
            var model = GenericModelFactory.CreateWithId<InputNormalizationModel>(runtimeType, blackboard);

            var intelligenceModel = RootViewModel.Model;
            model.InputContainer = intelligenceModel.Inputs;
            
            return model;
        }
    }
}