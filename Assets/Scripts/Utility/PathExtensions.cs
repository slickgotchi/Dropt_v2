using System.Collections.Generic;
using System.Linq;
using AI.NPC.Path;
using UnityEngine;

namespace Dropt.Utils
{
    public static class PathExtensions
    {
        public static Vector3[] ToWoldPositions(this IEnumerable<PathPointView> source)
        {
            return source.Where(temp => temp != null).Select(temp => temp.position).ToArray();
        }
    }
}