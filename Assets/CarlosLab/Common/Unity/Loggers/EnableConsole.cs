#region

using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#endregion

namespace CarlosLab.Common
{
    public abstract class EnableConsole : MonoBehaviour
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        public static void Init()
        {
            if (!Console.IsInitialized)
            {
                ILogger logger = new UnityLogger();
                Console.Init(logger);
            }
        }
    }

    public abstract class EnableConsole<T> : EnableConsole where T : Console<T>
    {
        [SerializeField]
        private bool isEnabled;

        private T instance;

        private T Instance
        {
            get
            {
                if (instance == null)
                {
                    PropertyInfo property = typeof(T).BaseType?.GetProperty(nameof(Console<T>.Instance),
                        BindingFlags.Public | BindingFlags.Static);
                    object propertyValue = property?.GetValue(null);
                    instance = propertyValue as T;
                }

                return instance;
            }
        }

        private void OnValidate()
        {
            UpdateEnableState();
        }

        private void UpdateEnableState()
        {
            Instance.IsEnabled = isEnabled;
        }
    }
}