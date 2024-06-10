using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<TargetFilterContainerModel, TargetFilterModel>))]
    public class TargetFilterContainerModel: ItemContainerModel<TargetFilterModel, TargetFilter, TargetFilterContainer>
    {
    }
}