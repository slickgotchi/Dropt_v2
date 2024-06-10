#region

using System;
using System.Runtime.Serialization;
using CarlosLab.Common;
using Newtonsoft.Json;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<InputModel>))]
    public class InputModel : GenericModelItem<Input>, IContainerItemValue
    {
        public Type ValueType => Runtime.ValueType;
        public object ValueObject => Runtime.ValueObject;
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is UtilityIntelligenceAsset asset)
            {
                asset.Inputs.Add(this);
            }
        }
    }
}