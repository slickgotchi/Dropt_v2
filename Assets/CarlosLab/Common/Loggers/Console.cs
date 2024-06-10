#region

using System;
using System.Diagnostics;

#endregion

namespace CarlosLab.Common
{
    public abstract class Console
    {
        private static ILogger logger;
        private static bool isInitialized;

        private readonly string tag;

        protected Console()
        {
            tag = GetTag();
        }

        public static bool IsInitialized => isInitialized;
        public bool IsEnabled { get; set; }

        public static void Init(ILogger newLogger)
        {
            logger = newLogger;
            isInitialized = true;
        }

        protected virtual string GetTag()
        {
            return GetType().Name;
        }

        [Conditional("CARLOSLAB_ENABLE_LOG")]
        public void Log(string message)
        {
            if (!isInitialized)
                return;

            if (!IsEnabled)
                return;

            if (string.IsNullOrEmpty(tag))
                logger.Log(message);
            else
                logger.Log(tag, message);
        }

        public void LogWarning(string message)
        {
            if (!isInitialized)
                return;

            if (string.IsNullOrEmpty(tag))
                logger.LogWarning(message);
            else
                logger.LogWarning(tag, message);
        }

        public void LogError(string message)
        {
            if (!isInitialized)
                throw new InvalidOperationException("Console is not initialized yet.");

            if (string.IsNullOrEmpty(tag))
                logger.LogError(message);
            else
                logger.LogError(tag, message);
        }

        public void LogException(Exception exception)
        {
            if (!isInitialized)
                return;

            logger.LogException(exception);
        }
    }

    public abstract class Console<T> : Console where T : Console<T>
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