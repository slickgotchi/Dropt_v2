#region

using System;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputListViewModel : ContainerViewModel<InputContainerModel, InputModel, InputItemViewModel>
    {
        protected override InputModel CreateModel(Type runtimeType)
        {
            var blackboard = Model.Runtime.Blackboard;
            var model = GenericModelFactory.CreateWithId<InputModel>(runtimeType, blackboard);

            return model;
        }
    }
}