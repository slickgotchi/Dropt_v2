using CarlosLab.Common;
using Newtonsoft.Json;

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<InputNormalizationContainerModel, InputNormalizationModel>))]
    public class InputNormalizationContainerModel : ItemContainerModel<InputNormalizationModel, InputNormalization, InputNormalizationContainer>
    {
        
    }
}