using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarlosLab.Common
{
    public static class DataSerializer
    {
        private static readonly JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Error,
            SerializationBinder = new IgnoreAssembliesSerializationBinder(),
            ContractResolver = new JsonContractResolver()
        };
        
        public static string Serialize(object obj, StreamingContext context)
        {
            string json = string.Empty;
            if (obj == null)
                return json;
            
            try
            {
                settings.Context = context;
                json = JsonConvert.SerializeObject(obj, settings);
            }
            catch (JsonException e)
            {
                CommonConsole.Instance.LogException(e);
            }

            return json;
        }

        public static string Format(string serializedModel)
        {
            return JToken.Parse(serializedModel).ToString(Formatting.Indented);
        }
        
        public static T Deserialize<T>(string json, StreamingContext context)
        {
            T obj = default;
            if (string.IsNullOrEmpty(json))
                return obj;

            try
            {
                settings.Context = context;
                obj = JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (JsonException e)
            {
                CommonConsole.Instance.LogException(e);
            }

            return obj;
        }
    }
}