#region

using CarlosLab.Common;
using Newtonsoft.Json;

#endregion

namespace CarlosLab.Common
{
    [JsonConverter(typeof(ItemContainerConverter<BlackboardModel, Variable>))]
    public class BlackboardModel : ItemContainerModel<Variable, Blackboard>
    {
    }
}