using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<DecisionMakerContainerModel, DecisionMakerModel>))]
    public class DecisionMakerContainerModel : ItemContainerModel<DecisionMakerModel, DecisionMaker, DecisionMakerContainer>
    {
        
    }
}