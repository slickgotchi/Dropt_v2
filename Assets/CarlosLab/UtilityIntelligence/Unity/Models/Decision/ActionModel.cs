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
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is UtilityIntelligenceAsset asset)
            {
                asset.Actions.Add(this);
            }
        }
    }
}