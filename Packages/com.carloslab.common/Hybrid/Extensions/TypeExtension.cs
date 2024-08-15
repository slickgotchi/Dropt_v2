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
            List<FieldInfo> totalFields = new();
            while (type != null)
            {
                var currentFields = type.GetFields(bindingFlags);
                totalFields.InsertRange(0, currentFields);
                type = type.BaseType;
            }

            return totalFields;
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
        
        public static object GetDefaultValue(this Type type)
        {
            object defaultValue = type.IsClass ? null : Activator.CreateInstance(type);
            return defaultValue;
        }
        
        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
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