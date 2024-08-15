#region

using System.Runtime.Serialization;
using CarlosLab.Common;
using Newtonsoft.Json;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<ActionModel>))]
    public class ActionModel : GenericModelWithId<ActionTask>
    {

    }
}