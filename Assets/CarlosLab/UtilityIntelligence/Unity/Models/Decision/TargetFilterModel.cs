#region

using System.Runtime.Serialization;
using CarlosLab.Common;
using Newtonsoft.Json;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<TargetFilterModel>))]
    public class TargetFilterModel : GenericModelItem<TargetFilter>
    {
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is UtilityIntelligenceAsset asset)
            {
                asset.TargetFilters.Add(this);
            }
        }
    }
}