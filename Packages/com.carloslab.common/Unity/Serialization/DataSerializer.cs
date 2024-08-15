using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CarlosLab.Common
{
    public static class DataSerializer
    {
        private static readonly JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            SerializationBinder = new IgnoreAssembliesSerializationBinder(),
            ContractResolver = new JsonContractResolver()
        };

        public static bool TrySerialize(object model, StreamingContext context, out string serializedModel)
        {
            serializedModel = null;
            if (model == null)
                return false;
            
            try
            {
                settings.Context = context;
                serializedModel = JsonConvert.SerializeObject(model, settings);
                return true;
            }
            catch (JsonException e)
            {
                StaticConsole.LogException(e);
            }

            return false;
        }

        public static string Format(string serializedModel)
        {
            return JToken.Parse(serializedModel).ToString(Formatting.Indented);
        }
        
        public static bool TryDeserialize<T>(string json, StreamingContext context, out T model)
        {
            model = default;
            if (string.IsNullOrEmpty(json))
                return false;
            
            try
            {
                settings.Context = context;
                model = JsonConvert.DeserializeObject<T>(json, settings);
                return true;
            }
            catch (JsonException e)
            {
                StaticConsole.LogException(e);
            }

            return false;
        }
    }
}