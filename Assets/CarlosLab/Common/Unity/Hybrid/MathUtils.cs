#region

#if UNITY_MATHEMATICS
using Unity.Mathematics;
#endif

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public static class MathUtils
    {
        public const float Epsilon = 1e-5f;

        public static float DistanceSquared(Vector3 startPoint, Vector3 endPoint)
        {
#if UNITY_MATHEMATICS
            return math.distancesq(startPoint, endPoint);

#else
            return (endPoint - startPoint).sqrMagnitude;
#endif
        }
        
        public static float Distance(Vector3 startPoint, Vector3 endPoint)
        {
#if UNITY_MATHEMATICS
            return math.distance(startPoint, endPoint);
#else
            return Vector3.Distance(startPoint, endPoint);
#endif
        }

        public static float Pow(float x, float y)
        {
#if UNITY_MATHEMATICS
            return math.pow(x, y);
#else
            return Mathf.Pow(x, y);
#endif
        }

        public static float Exp(float x)
        {
#if UNITY_MATHEMATICS
            return math.exp(x);
#else
            return Mathf.Exp(x);
#endif
        }

        public static float Log(float x)
        {
#if UNITY_MATHEMATICS
            return math.log(x);
#else
            return Mathf.Log(x);
#endif
        }

        public static float Sin(float x)
        {
#if UNITY_MATHEMATICS
            return math.sin(x);
#else
            return Mathf.Sin(x);
#endif
        }
    }
}
