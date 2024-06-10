#region

using System;

#endregion

namespace CarlosLab.Common.UI
{
    public static class ModelFactory<TModel> where TModel : class
    {
        public static TModel Create(Type type, params object[] args)
        {
            if (type == null)
                type = typeof(TModel);

            // if (args == null)
            //     args = new object[] { null };

            object instance = Activator.CreateInstance(type, args);
            return instance as TModel;
        }
    }
}