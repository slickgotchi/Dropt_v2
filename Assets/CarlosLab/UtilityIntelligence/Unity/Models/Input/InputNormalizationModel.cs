#region

using System;
using System.Runtime.Serialization;
using CarlosLab.Common;
using Newtonsoft.Json;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<InputNormalizationModel>))]
    public class InputNormalizationModel : GenericModel<InputNormalization>
    {
        public Type ValueType => Runtime.ValueType;
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is UtilityIntelligenceAsset asset)
            {
                asset.InputNormalizations.Add(this);
            }
        }
    }
}