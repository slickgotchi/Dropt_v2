using UnityEngine;

namespace Assets.Plugins
{
    public static class Defines
    {
        public static bool FAST_START
        {
            get
            {
                bool result = false;
#if UNITY_EDITOR
                string path = Application.dataPath + "/../Library/";
                result = System.IO.File.Exists(path + "FAST_START");
#endif

                return result;
            }
        }
    }
}