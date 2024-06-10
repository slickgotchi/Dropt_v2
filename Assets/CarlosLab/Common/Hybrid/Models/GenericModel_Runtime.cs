using System;
using System.Collections.Generic;

namespace CarlosLab.Common
{
    public abstract class GenericModel<TRuntime> : GenericModel, IModel<TRuntime>
        where TRuntime : class
    {
        private TRuntime runtime;

        public override object RuntimeObject => Runtime;

        public TRuntime Runtime
        {
            get
            {
                if (runtime == null)
                    runtime = CreateRuntimeObject();
                return runtime;
            }
        }

        public override Type RuntimeBaseType => typeof(TRuntime);

        public override void SetValueRuntime(string fieldName, object value)
        {
            if (Initialized == false)
                return;

            SetValue(runtime, fieldName, value);
        }

        public override object GetValueRuntime(string fieldName)
        {
            if (Initialized == false)
                return null;

            return GetValue(runtime, fieldName);
        }

        private TRuntime CreateRuntimeObject()
        {
            if (!Initialized)
                return null;

            object runtime = Activator.CreateInstance(RuntimeType);
            foreach (KeyValuePair<string, object> field in FieldValues)
            {
                SetValue(runtime, field.Key, field.Value);
            }

            return runtime as TRuntime;
        }
    }
}