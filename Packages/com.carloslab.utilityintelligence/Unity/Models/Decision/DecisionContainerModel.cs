using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<DecisionContainerModel, DecisionModel>))]
    public class DecisionContainerModel : ItemContainerModel<DecisionModel, Decision, DecisionContainer>
    {
    }
}