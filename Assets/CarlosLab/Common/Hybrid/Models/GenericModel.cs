#region

using CarlosLab.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

#endregion

namespace CarlosLab.Common
{
    public abstract class GenericModel : IModel
    {
        #region Fields

        private readonly Dictionary<string, FieldInfo> publicFieldInfos = new();
        private readonly Dictionary<string, FieldInfo> fieldInfos = new();
        private readonly Dictionary<string, object> fieldValues = new();
        private Type runtimeType;
        private bool initialized;

        #endregion

        #region Properties

        public bool Initialized => initialized;

        public IReadOnlyDictionary<string, object> FieldValues => fieldValues;
        public IReadOnlyDictionary<string, FieldInfo> FieldInfos => fieldInfos;
        public IReadOnlyDictionary<string, FieldInfo> PublicFieldInfos => publicFieldInfos;

        public Type RuntimeType => runtimeType;
        
        public abstract Type RuntimeBaseType { get; }

        public abstract object RuntimeObject { get; }

        #endregion

        #region Model Functions

        public bool Init(Type runtimeType)
        {
            if (initialized || runtimeType == null) return false;

            if (!RuntimeBaseType.IsAssignableFrom(runtimeType)) return false;
            
            this.runtimeType = runtimeType;

            InitRuntime();
            
            OnInit();

            initialized = true;

            return true;
        }

        private void InitRuntime()
        {
            Type runtimeType = RuntimeType;
            object runtimeObject = Activator.CreateInstance(RuntimeType);
            AddPublicFields(runtimeType, runtimeObject);
            AddNonPublicFields(runtimeType, runtimeObject);
        }

        private void AddPublicFields(Type runtimeType, object runtimeObject)
        {
            FieldInfo[] publicFields = runtimeType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in publicFields)
            {
                AddField(field.Name, field, field.GetValue(runtimeObject));
                publicFieldInfos.TryAdd(field.Name, field);
            }
        }
        
        private void AddNonPublicFields(Type runtimeType, object runtimeObject)
        {
            List<FieldInfo> nonPublicFields = runtimeType.GetAllFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo field in nonPublicFields)
            {
                DataMemberAttribute dataMemberAttribute = field.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttribute == null)
                    continue;

                AddField(dataMemberAttribute.Name, field, field.GetValue(runtimeObject));
            }
        }

        protected virtual void OnInit()
        {
            
        }

        private void AddField(string fieldName, FieldInfo fieldInfo, object fieldValue)
        {
            if (fieldInfos.ContainsKey(fieldName)) return;

            fieldInfos.Add(fieldName, fieldInfo);
            fieldValues.Add(fieldName, fieldValue);

            OnFieldAdded(fieldInfo);
        }

        protected virtual void OnFieldAdded(FieldInfo fieldInfo)
        {
        }

        #endregion

        #region Get/Set Functions

        public void SetValue(string fieldName, object value)
        {
            SetValueWithoutRuntime(fieldName, value);
            SetValueRuntime(fieldName, value);
        }

        public void SetValueWithoutRuntime(string fieldName, object value)
        {
            if (!initialized)
                return;

            if (fieldValues.ContainsKey(fieldName))
                fieldValues[fieldName] = value;
        }

        public object GetValue(string fieldName)
        {
            if (!initialized)
                return null;

            return fieldValues.GetValueOrDefault(fieldName);
        }

        public abstract void SetValueRuntime(string fieldName, object value);
        public abstract object GetValueRuntime(string fieldName);

        protected void SetValue(object target, string fieldName, object value)
        {
            if (target == null || string.IsNullOrEmpty(fieldName))
                return;

            if (!fieldInfos.TryGetValue(fieldName, out FieldInfo fieldInfo))
                return;

            fieldInfo.SetValue(target, value);
        }

        protected object GetValue(object target, string fieldName)
        {
            if (target == null || string.IsNullOrEmpty(fieldName))
                return null;

            if (!fieldInfos.TryGetValue(fieldName, out FieldInfo fieldInfo))
                return null;

            return fieldInfo.GetValue(target);
        }

        #endregion
    }
}