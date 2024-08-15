#region

using System;

#endregion

namespace CarlosLab.Common
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new();

                return instance;
            }
        }
    }
}