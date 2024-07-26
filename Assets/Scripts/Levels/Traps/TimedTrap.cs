using System.Linq;
using UnityEngine;

namespace Level.Traps
{
    public sealed class TimedTrap : DamagedTrap
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
        
        private void TryToInitTimings()
        {
            if (null != m_timings)
                return;

            //calculate duration till damage frame
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

            //calculate frame timings taking into account group number and damage frame time offset
            m_minTime = Group.Value * m_timeToGoToNextGroup;
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

            TryToInitTimings();

            //switch between trap frames
            m_time += Mathf.RoundToInt(Time.deltaTime * 1000);
            m_frame = GetFrameByTime(m_time);
            m_trapImage.sprite = m_frames[m_frame].Frame;

            var maxGroupsCount = MaxGroup.Value;

            //reset time when animation is over
            if (Group.Value + 1 == maxGroupsCount)
            {
                if (m_time >= maxGroupsCount * m_totalDuration)
                {
                    m_time = m_timeToGoToNextGroup;
                }
            }
            else
            if (m_time >= maxGroupsCount * m_totalDuration - m_timeToGoToNextGroup)
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