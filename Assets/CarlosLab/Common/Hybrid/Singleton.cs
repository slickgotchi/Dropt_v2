﻿#region

using System;

#endregion

namespace CarlosLab.Common
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = Activator.CreateInstance(typeof(T), true) as T;

                return instance;
            }
        }
    }
}