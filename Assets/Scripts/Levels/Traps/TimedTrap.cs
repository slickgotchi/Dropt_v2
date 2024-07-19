using System.Linq;
using UnityEngine;

namespace Level.Traps
{
    public sealed class TimedTrap : Trap
    {
        [SerializeField] private SpriteRenderer m_trapImage;
        [SerializeField] private TimedTrapFrame[] m_frames;
        private int[] m_timings;

        protected override bool IsAvailableForAttack
        {
            get
            {
                return m_frames[m_frame].IsActive;
            }
        }

        private int m_totalDuration;
        private int m_timeToGoToNextGroup;
        private int m_time;
        private int m_frame;
        private int m_minTime;
        private int m_maxTime;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
        }

        public override void SetupGroup(TrapsGroupSpawner spawner, int group)
        {
            base.SetupGroup(spawner, group);

            if (!IsServer) return;

            m_timeToGoToNextGroup = 0;
            for (int i = 0; i < m_frames.Length; i++)
            {
                if (m_frames[i].IsGotoToNextGroup)
                {
                    var frame = m_frames[i];
                    frame.TimeMs = 0;
                    m_frames[i] = frame;
                    break;
                }

                m_timeToGoToNextGroup += m_frames[i].TimeMs;
            }

            m_totalDuration = m_frames.Sum(temp => temp.TimeMs);

            m_minTime = m_currentGroup * m_timeToGoToNextGroup;
            m_timings = new int[m_frames.Length];

            m_maxTime = m_minTime;
            for (int i = 0; i < m_frames.Length; i++)
            {
                m_maxTime += m_frames[i].TimeMs;

                if (m_frames[i].IsGotoToNextGroup)
                {
                    m_maxTime += m_timeToGoToNextGroup;
                }

                m_timings[i] = m_maxTime;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!IsServer)
                return;

            m_time += Mathf.RoundToInt(Time.deltaTime * 1000);
            m_frame = GetFrameByTime(m_time);
            m_trapImage.sprite = m_frames[m_frame].Frame;

            if (m_currentGroup + 1 == m_group.MaxGroupsCount)
            {
                if (m_time >= m_group.MaxGroupsCount * m_totalDuration)
                {
                    m_time = m_timeToGoToNextGroup;
                }
            }
            else
            if (m_time >= m_group.MaxGroupsCount * m_totalDuration - m_timeToGoToNextGroup)
            {
                m_time = 0;
            }
        }

        private int GetFrameByTime(int time)
        {
            if (time < m_minTime || time > m_maxTime)
                return 0;

            for (int i = 0; i < m_timings.Length; i++)
            {
                if (time - m_timings[i] <= 0)
                {
                    return i;
                }
            }

            return m_frames.Length - 1;
        }
    }
}