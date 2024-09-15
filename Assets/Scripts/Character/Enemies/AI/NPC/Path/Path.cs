using Dropt.Utils;
using UnityEngine;

namespace AI.NPC.Path
{
    public interface IPath
    {
        Vector3[] Positions { get; }
        float TotalDistance { get; }
        Vector3 GetPosition(float progress);
        float GetAngle(float progress);
    }

    public sealed class Path : IPath
    {
        private Vector3[] _positions;
        private float[] _distances;
        private float _totalDistance;

        public float TotalDistance => _totalDistance;

        public Vector3[] Positions => _positions;

        public Path(params Vector3[] positions)
        {
            _positions = positions;

            for (int i = _positions.Length - 2; i >= 0; i--)
            {
                if (Vector3.Distance(_positions[i], _positions[i + 1]) < .01f)
                {
                    _positions = _positions.RemoveAt(i);
                }
            }

            _totalDistance = 0;
            _distances = new float[_positions.Length];

            for (int i = 0; i < _positions.Length - 1; i++)
            {
                _totalDistance += Vector3.Distance(_positions[i], _positions[i + 1]);
                _distances[i] = _totalDistance;
            }
        }

        public Vector3 GetPosition(float distance)
        {
            int index = GetIndexByDistance(distance, out var factor);
            return Vector3.Lerp(_positions[index], _positions[index + 1], factor);
        }

        public float GetAngle(float distance)
        {
            int index = GetIndexByDistance(distance, out var factor);
            return GetAngle(_positions[index + 1], _positions[index]);
        }

        private float GetAngle(Vector3 start, Vector3 end)
        {
            return Mathf.Atan2(start.y - end.y, start.x - end.x) * Mathf.Rad2Deg ;
        }

        private int GetIndexByDistance(float distance, out float factor)
        {
            var previousTotalDistance = 0f;

            for (int i = 0; i < _distances.Length; i++)
            {
                if (distance <= _distances[i])
                {
                    factor = (distance - previousTotalDistance) / (_distances[i] - previousTotalDistance);
                    return i;
                }

                previousTotalDistance = _distances[i];
            }

            factor = (distance - previousTotalDistance) / (_distances[_distances.Length - 1] - previousTotalDistance);
            return _distances.Length - 2;
        }
    }
}