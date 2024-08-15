#region

using CarlosLab.Common;
using Newtonsoft.Json;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(ItemContainerConverter<InputContainerModel, InputModel>))]
    public class InputContainerModel : ItemContainerModel<InputModel, Input, InputContainer>
    {
    }
}