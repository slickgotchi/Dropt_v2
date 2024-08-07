using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dropt.Utils;
using UnityEngine;

namespace Audio
{
    public partial class AudioManager
    {
        #region Properties
        public bool MuteMusic
        {
            set
            {
                foreach (var source in m_musicChannels)
                {
                    source.mute = value;
                }

                m_muteMusic = value;
            }
            get => m_muteMusic;
        }

        public float VolumeMusic
        {
            set
            {
                foreach (var source in m_musicChannels)
                {
                    if (source.loop)
                    {
                        source.volume = value;
                    }
                }

                m_volumeMusic = value;
            }
            get => m_volumeMusic;
        }

        #endregion

        #region Methods

        public void PlayMusic(MusicType[] types)
        {
            string[] names = new string[types.Length];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = types[i].ToString();
            }
            PlayMusic(names);
        }

        public async void PlayMusic(string[] names)
        {
            if (m_musicChannels.CountFreeChannels() < names.Length)
            {
                if (kShowLogs)
                {
                    Debug.LogWarning("No free channels");
                }
                return;
            }

            await PreloadAsync(names);

            var tasks = new List<UniTask>();

            if (m_musicChannels.TryGetBusyChannels(out var busyChannels))
            {
                foreach (var busyChannel in busyChannels)
                {
                    tasks.Add(busyChannel.FadeOut(m_config.FadingDuration, kShowLogs));
                }
            }

            foreach (var name in names)
            {
                m_musicChannels.TryGetFreeChannel(out var channel);

                if (null == channel)
                    break;

                await PlayAsync(name, channel);

                tasks.Add(channel.FadeIn(m_config.FadingDuration, kShowLogs));
            }

            await UniTask.WhenAll(tasks);
        }

        public bool IsMusicPlay(MusicType type)
        {
            return IsMusicPlay(type.ToString());
        }

        public bool IsMusicPlay(string name)
        {
            return m_musicChannels.CountPlayingDuplicates(name) > 0;
        }

        public void StopMusic(MusicType type)
        {
            StopMusic(type.ToString());
        }

        public void StopMusic(string name)
        {
            foreach (var audioSource in m_musicChannels)
            {
                if (audioSource.clip != null && string.Equals(audioSource.clip.name, name))
                {
                    audioSource.StopSingle();
                }
            }
        }

        #endregion
    }
}