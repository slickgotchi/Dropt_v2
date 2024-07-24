using UnityEngine;

namespace Level.Traps
{
    public sealed class TimedTrap : DamagedTrap
    {
        [SerializeField] private SpriteRenderer m_trapImage;
        [SerializeField] private TimedTrapFrame[] m_frames;

        protected override bool IsAvailableForAttack
        {
            get
            {
                return m_frames[m_frame].IsActive;
            }
        }

        private int m_timeToGoToNextGroup;
        private int m_time;
        private int m_frame;

        private void TryToInitTimings()
        {
            if (m_timeToGoToNextGroup > 0)
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
        }

        protected override void Update()
        {
            base.Update();

            TryToInitTimings();

            var maxGroupsCount = MaxGroup.Value;

            var ticks = Mathf.Repeat(NetworkManager.ServerTime.TimeAsFloat * 1000f - Group.Value * m_timeToGoToNextGroup, (maxGroupsCount) * m_timeToGoToNextGroup);
            m_time = Mathf.RoundToInt(ticks);

            //switch between trap frames
            m_frame = GetFrameByTime(m_time);
            m_trapImage.sprite = m_frames[m_frame].Frame;
        }

        private int GetFrameByTime(int time)
        {
            for (int i = 0; i < m_frames.Length; i++)
            {
                if (m_frames[i].IsGotoToNextGroup)
                {
                    time -= m_timeToGoToNextGroup;
                }
                else
                {
                    time -= m_frames[i].TimeMs;
                }

                if (time <= 0)
                {
                    return i;
                }
            }

            return m_frames.Length - 1;
        }
    }
}