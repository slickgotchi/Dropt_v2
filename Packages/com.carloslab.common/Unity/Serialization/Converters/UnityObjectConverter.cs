#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

#endregion

namespace CarlosLab.Common
{
    public class UnityObjectConverter : JsonConverter
    {
        private readonly List<Object> objects;

        public UnityObjectConverter(List<Object> objects)
        {
            this.objects = objects;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            if (value is Object unityObject)
            {
                writer.WriteValue(unityObject.GetInstanceID());
                return;
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return new object();

            // var list = serializer.Deserialize<IList<TValue>>(reader);
            // var dict = new Dictionary<TKey, TValue>();
            // foreach (var t in list) {
            //     dict[getKeyFromValue(t)] = t;
            // }
            // return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            bool canConvert = typeof(Object).IsAssignableFrom(objectType);
            return canConvert;
        }
    }
}