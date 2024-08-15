using UnityEngine;

namespace CarlosLab.Common
{
    public static class DebugUtils
    {
        public static void DrawPoint(Vector3 position, Color color, float scale = 1.0f, float duration = 0, bool depthTest = true)
        {
            Debug.DrawRay(position+(Vector3.up*(scale*0.5f)), -Vector3.up*scale, color, duration, depthTest);
            Debug.DrawRay(position+(Vector3.right*(scale*0.5f)), -Vector3.right*scale, color, duration, depthTest);
            Debug.DrawRay(position+(Vector3.forward*(scale*0.5f)), -Vector3.forward*scale, color, duration, depthTest);
        }
    }
}