#region

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace CarlosLab.Common.Extensions
{
    public static class TypeExtension
    {
        public static List<FieldInfo> GetAllFields(this Type type, BindingFlags bindingFlags)
        {
            List<FieldInfo> fields = new();
            while (type != null)
            {
                fields.AddRange(type.GetFields(bindingFlags));
                type = type.BaseType;
            }

            return fields;
        }

        public static bool IsSubclassOfGeneric(this Type type, Type genericTypeDefinition)
        {
            do
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;

                type = type.BaseType;
            } while (type != null);

            return false;
        }

        public static Type GetFirstGenericArgument(this Type type)
        {
            if (type.IsGenericType)
            {
                Type[] genericArguments = type.GetGenericArguments();
                if (genericArguments.Length > 0) return genericArguments[0];
            }

            return null;
        }

        public static string GetName(this Type type)
        {
            string typeName = type.Name;
            switch (type.Name)
            {
                case "Single":
                    typeName = "Float";
                    break;
                case "Int32":
                    typeName = "Int";
                    break;
                case "Int64":
                    typeName = "Long";
                    break;
                case "Boolean":
                    typeName = "Bool";
                    break;
            }

            return typeName;
        }
    }
}