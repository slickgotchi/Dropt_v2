#region

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace CarlosLab.Common
{
    public class JsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo) property.ShouldSerialize = _ => false;

            return property;
        }
    }
    public class JsonContractResolver<TAsset> : DefaultContractResolver
        where TAsset : IDataAsset
    {
        private TAsset asset;

        public JsonContractResolver(TAsset asset)
        {
            this.asset = asset;
        }

        public TAsset Asset
        {
            get => asset;
            set => asset = value;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo) property.ShouldSerialize = _ => false;

            return property;
        }
    }
}