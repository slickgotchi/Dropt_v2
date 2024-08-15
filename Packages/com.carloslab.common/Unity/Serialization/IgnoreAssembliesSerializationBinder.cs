#region

using System;
using Newtonsoft.Json.Serialization;

#endregion

namespace CarlosLab.Common
{
    public sealed class IgnoreAssembliesSerializationBinder : ISerializationBinder
    {
        #region ISerializationBinder

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            // Note: Setting the assemblyName to null here will only remove it from the main type itself -
            // it won't remove it from any types specified as generic type parameters (that's what the
            // RemoveAssemblyNames method is needed for)
            assemblyName = null;
            typeName = TypeUtils.RemoveAssemblyNames(serializedType.FullName);
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            Type type = TypeUtils.GetType(typeName);
            return type;
        }

        #endregion
    }
}