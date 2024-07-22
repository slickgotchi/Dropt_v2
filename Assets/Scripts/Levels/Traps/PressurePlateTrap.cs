using System;
using Unity.Netcode;
using UnityEngine;

namespace Level.Traps
{
    public sealed class PressurePlateTrap : DamagedTrap
    {
        [HideInInspector]
        public NetworkVariable<bool> IsActive;

        [SerializeField] private SpriteRenderer m_trapImage;
        [SerializeField] private TimedTrapFrame[] m_frames;

        private int m_time;
        private int m_frame;
        private int m_activeFrame;
        private int m_disableFrame;

        protected override bool IsAvailableForAttack
        {
            get
            {
                return m_frames[m_frame].IsActive;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_disableFrame = 0;
            m_activeFrame = Array.FindLastIndex(m_frames, temp => temp.IsGotoToNextGroup);
        }

        protected override void Update()
        {
            base.Update();

            var frame = (IsActive.Value) ? m_activeFrame : m_disableFrame;
            
            if (frame == m_frame)
                return;

            //switch between trap frames
            m_time += Mathf.RoundToInt(Time.deltaTime * 1000);
            m_frame = GetFrameByTime(m_time);

            if (m_frame >= m_frames.Length - 1)
            {
                m_time = 0;
            }

            m_trapImage.sprite = m_frames[m_frame].Frame;
        }

        private int GetFrameByTime(int time)
        {
            for (int i = 0; i < m_frames.Length; i++)
            {
                time -= m_frames[i].TimeMs;
                if (time <= 0)
                {
                    return i;
                }
            }

            return m_frames.Length - 1;
        }
    }
}