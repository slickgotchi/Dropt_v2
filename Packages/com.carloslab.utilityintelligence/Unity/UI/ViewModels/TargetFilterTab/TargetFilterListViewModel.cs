using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListViewModel :
        ContainerViewModel<TargetFilterContainerModel, TargetFilterModel, TargetFilterItemViewModel>
    {
        protected override TargetFilterModel CreateModel(Type runtimeType)
        {
            var blackboard = Model.Runtime.Blackboard;
            var model = GenericModelFactory.CreateWithId<TargetFilterModel>(runtimeType, blackboard);
            return model;
        }
    }
}