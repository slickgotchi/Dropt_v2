#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#endregion

namespace CarlosLab.Common
{
    public static class TypeUtils
    {
        private static Assembly[] assemblyCache;
        private static readonly Dictionary<string, Type> TypeCache = new();

        private static Assembly[] AssemblyCache
        {
            get
            {
                if (assemblyCache == null)
                    assemblyCache = AppDomain.CurrentDomain.GetAssemblies();

                return assemblyCache;
            }
        }

        public static Type GetType(string typeName)
        {
            if (TypeCache.TryGetValue(typeName, out Type type))
                return type;

            foreach (Assembly assembly in AssemblyCache)
            {
                type = GetType(assembly, typeName);
                if (type != null) break;
            }

            return type;
        }

        private static Type GetType(Assembly assembly, string typeName)
        {
            Type type = assembly.GetType(typeName);
            if (type == null)
                type = GetGenericType(assembly, typeName);

            if (type != null)
                TypeCache.Add(typeName, type);

            return type;
        }

        private static Type GetGenericType(Assembly assembly, string typeName)
        {
            Type type = null;
            int index = typeName.IndexOf("`", StringComparison.Ordinal);
            if (index != -1)
            {
                index += 2;
                string genericTypeName = typeName.Substring(0, index);

                string argumentTypeString = typeName.Substring(index + 1, typeName.Length - index - 1);

                string[] argumentTypeNames = argumentTypeString.Split(",");
                List<Type> argumentTypes = new();
                foreach (string argumentTypeName in argumentTypeNames)
                {
                    typeName = argumentTypeName.TrimStart('[').TrimEnd(']');
                    Type argumentType = GetType(typeName);

                    if (argumentType != null)
                        argumentTypes.Add(argumentType);
                }

                Type genericType = assembly.GetType(genericTypeName);

                if (genericType != null && argumentTypes.Count == argumentTypeNames.Length)
                    type = genericType.MakeGenericType(argumentTypes.ToArray());
            }

            return type;
        }

        public static string RemoveAssemblyNames(string typeName)
        {
            int index = 0;
            StringBuilder content = new();
            RecusivelyRemoveAssemblyNames();
            return content.ToString();

            void RecusivelyRemoveAssemblyNames()
            {
                // If we started inside a type name - eg.
                //
                //   "System.Int32, System.Private.CoreLib"
                //
                // .. then we want to look for the comma that separates the type name from the assembly
                // information and ignore that content. If we started inside nested generic type content
                // - eg.
                //
                //  "[System.Int32, System.Private.CoreLib], [System.String, System.Private.CoreLib]"
                //
                // .. then we do NOT want to start ignoring content after any commas encountered. So
                // it's important to know here which case we're in.
                bool insideTypeName = typeName[index] != '[';

                bool ignoreContent = false;
                while (index < typeName.Length)
                {
                    char c = typeName[index];
                    index++;

                    if (insideTypeName && c == ',')
                    {
                        ignoreContent = true;
                        continue;
                    }

                    if (!ignoreContent)
                        content.Append(c);

                    if (c == '[')
                        RecusivelyRemoveAssemblyNames();
                    else if (c == ']')
                    {
                        if (ignoreContent)
                            // If we encountered a comma that indicated that we were about to start
                            // an assembly name then we'll have stopped adding content to the string
                            // builder but we don't want to lose this closing brace, so explicitly
                            // add it in if that's the case
                            content.Append(c);

                        break;
                    }
                }
            }
        }
    }
}