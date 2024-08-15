#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CarlosLab.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class GenericModelConverter<TModel> : ModelConverter<TModel>
        where TModel : GenericModel, new()
    {
        private const string TypePropertyName = "$type";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TModel model = value as TModel;
            if (model == null)
                throw new Exception($"Cannot serialize {value.GetType().Name} as {typeof(GenericModel).Name}");

            OnSerializing(model, serializer.Context);

            writer.WriteStartObject();
            writer.WritePropertyName(TypePropertyName);
            serializer.Serialize(writer, model.RuntimeType.FullName);
            
            foreach (KeyValuePair<string, object> field in model.FieldValues)
            {
                writer.WritePropertyName(field.Key);

                object fieldValue;
                if (field.Value is LayerMask layerMask)
                    fieldValue = (int)layerMask;
                else
                    fieldValue = field.Value;
                    
                serializer.Serialize(writer, fieldValue);
            }

            writer.WriteEndObject();
            
            OnSerialized(model, serializer.Context);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            TModel model = new();
            
            OnDeserializing(model, serializer.Context);

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();
                if (propertyName == TypePropertyName)
                {
                    string typeName = reader.ReadAsString();
                    Type runtimeType = TypeUtils.GetType(typeName);
                    if (!model.Init(runtimeType))
                        throw new Exception($"Cannot deserialize {typeName}");
                }
                else
                {
                    reader.Read();

                    if (model.FieldInfos.TryGetValue(propertyName, out FieldInfo fieldInfo))
                    {
                        // Debug.Log($"ReadJson3 Type: {model.RuntimeType.Name} Property: {propertyName}");

                        object value;
                        if (fieldInfo.FieldType == typeof(LayerMask))
                        {
                            int layerMaskInt = (int) serializer.Deserialize(reader, typeof(int));
                            value = (LayerMask)layerMaskInt;
                        }
                        else
                        {
                            try
                            {
                                value = serializer.Deserialize(reader, fieldInfo.FieldType);
                            }
                            catch (JsonException)
                            {
                                value = fieldInfo.FieldType.GetDefaultValue();
                            }
                        }
                        
                        model.SetValueWithoutRuntime(propertyName, value);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                reader.Read();
            }

            OnDeserialized(model, serializer.Context);

            return model;
        }

        public override bool CanConvert(Type objectType)
        {
            bool canConvert = typeof(TModel).IsAssignableFrom(objectType);
            return canConvert;
        }
    }
}