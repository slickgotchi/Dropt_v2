#region

using System.Diagnostics;
using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public static class UndoUtils
    {
        [Conditional("UNITY_EDITOR")]
        public static void RecordObject(Object target, string name)
        {
#if UNITY_EDITOR
            // Debug.Log($"UndoUtility.RecordObject name: {name}");
            if (Application.isPlaying || target == null) return;
            UnityEditor.Undo.RecordObject(target, name);
#endif
        }
    }
}