using System;
using System.Linq;
using AI.NPC.Path;
using Dropt.Utils;
using Microsoft.IdentityModel.Tokens;
using Unity.Netcode;
using UnityEngine;
using Color = UnityEngine.Color;

namespace AI.NPC
{
    public class NpcMover : MonoBehaviour
    {
        [SerializeField] private float m_speed;
        [SerializeField] private PathPointView[] m_pathPoints;
        [SerializeField] private Color m_color;
        [SerializeField] private BodyAnimator m_animator;

        private Path.Path m_pathSystem;
        private float m_distance;
        private bool m_isMoving = true;
        private float m_stopStateTime;

        private PathPointView[] PathPoints => m_pathPoints;
        private float TotalDistance => m_pathSystem.TotalDistance;

        private void Awake()
        {
            m_pathSystem = new Path.Path(PathPoints.ToWoldPositions());
        }

        private void Update()
        {
            UpdateDust(m_isMoving);

            if (!m_isMoving)
            {
                UpdateStopDuration();
                return;
            }

            UpdateDistance();

            var position = m_pathSystem.GetPosition(m_distance);
            var angle = m_pathSystem.GetAngle(m_distance);

            var facing = GetFacing(angle);

            UpdatePosition(position);
            UpdateView(facing);
        }

        public void FaceTo(Transform target)
        {
            var directionFacing = target.position - transform.position;
            var facing = PlayerGotchi.Facing.NA;

            if (directionFacing.y < -Math.Abs(directionFacing.x)) facing = PlayerGotchi.Facing.Front;
            if (directionFacing.y > Math.Abs(directionFacing.x)) facing = PlayerGotchi.Facing.Back;
            if (directionFacing.x <= -Math.Abs(directionFacing.y)) facing = PlayerGotchi.Facing.Left;
            if (directionFacing.x >= Math.Abs(directionFacing.y)) facing = PlayerGotchi.Facing.Right;

            UpdateView(facing);
        }

        private void UpdateStopDuration()
        {
            m_stopStateTime += Time.deltaTime;
        }

        private void UpdatePosition(Vector3 position)
        {
            transform.position = position;
        }

        private void UpdateView(PlayerGotchi.Facing facing)
        {
            m_animator.UpdateFacing(facing);
        }

        private void UpdateDust(bool value)
        {
            m_animator.UpdateDusts(value);
        }

        private PlayerGotchi.Facing GetFacing(float angle)
        {
            angle %= 360;

            if (angle < 0)
            {
                angle += 360;
            }

            return angle switch
            {
                >= 315 or < 45 => PlayerGotchi.Facing.Right,
                >= 45 and < 135 => PlayerGotchi.Facing.Back,
                >= 135 and < 225 => PlayerGotchi.Facing.Left,
                >= 225 and < 315 => PlayerGotchi.Facing.Front,
                _ => PlayerGotchi.Facing.NA,
            };
        }

        private void UpdateDistance()
        {
            m_distance = m_speed * (NetworkManager.Singleton.LocalTime.TimeAsFloat - m_stopStateTime);
            m_distance = Mathf.Repeat(m_distance, TotalDistance);
        }

        public void Stop()
        {
            m_isMoving = false;
        }

        public void Resume()
        {
            m_isMoving = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (PathPoints.IsNullOrEmpty() || PathPoints.Any(temp => temp == null))
            {
                return;
            }

            var lastPointIndex = PathPoints.Length - 1;

            var isLooped = PathPoints[0] == PathPoints[lastPointIndex];

            for (var i = 0; i < PathPoints.Length; i++)
            {
                Gizmos.color = m_color;

                var point = PathPoints[i];

                var hasNext = i + 1 <= lastPointIndex;

                if (hasNext)
                {
                    var nextPoint = PathPoints[i + 1];
                    Gizmos.DrawLine(point.position, nextPoint.position);
                }
                else
                {
                    if (isLooped)
                    {
                        var lastPoint = PathPoints[lastPointIndex];
                        Gizmos.DrawLine(lastPoint.position, point.position);
                    }
                }

                Gizmos.DrawSphere(point.position, 0.5f);

                Gizmos.color = Color.white;
            }
        }
#endif
    }
}