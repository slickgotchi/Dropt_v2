#region

using CarlosLab.Common;
using Newtonsoft.Json;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<ConsiderationContainerModel, ConsiderationModel>))]
    public class ConsiderationContainerModel : ItemContainerModel<ConsiderationModel, Consideration, ConsiderationContainer>
    {
    }
}