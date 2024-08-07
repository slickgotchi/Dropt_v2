using Dropt.Utils;
using UnityEngine;

namespace Audio
{
    public sealed partial class AudioManager
    {
        #region Properties
        public bool MuteSounds
        {
            set
            {
                foreach (var source in m_soundChannels)
                {
                    source.mute = value;
                }

                m_muteSounds = value;
            }
            get => m_muteSounds;
        }

        public float VolumeSounds
        {
            set
            {
                foreach (var source in m_soundChannels)
                {
                    if (!source.loop)
                    {
                        source.volume = value;
                    }
                }

                m_volumeSounds = value;
            }
            get => m_volumeSounds;
        }

        #endregion

        #region Methods

        public void PlaySound(SoundType type, Vector3 position = default)
        {
            PlaySound(type.ToString(), position);
        }

        public void PlaySound(string name, Vector3 position = default)
        {
            if (CountPlayingDuplicates(name) >= m_config.MaxNumberOfDuplicates)
            {
                if (kShowLogs)
                {
                    Debug.LogWarning("Duplicates max is reached");
                }
                return;
            }

            if (m_soundChannels.TryGetFreeChannel(out var channel))
            {
                channel.transform.position = position;
                PlayWithStop(name, channel);
                return;
            }

            if (kShowLogs)
            {
                Debug.LogWarning("No free channels for sound");
            }
        }

        public void StopSound(SoundType type)
        {
            StopSound(type.ToString());
        }

        public void StopSound(string name)
        {
            foreach (var audioSource in m_soundChannels)
            {
                if (audioSource.clip != null && string.Equals(audioSource.clip.name, name))
                {
                    audioSource.StopSingle();
                    audioSource.transform.position = Vector3.zero;
                }
            }
        }

        private int CountPlayingDuplicates(string name)
        {
            return m_soundChannels.CountPlayingDuplicates(name);
        }

        #endregion
    }
}