using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AssetsManagement;
using Cysharp.Threading.Tasks;
using Dropt.Utils;
using Microsoft.IdentityModel.Tokens;
using UnityEngine;

namespace Audio
{
    public sealed partial class AudioManager
    {
        #region Fields

        private const bool kShowLogs = true;
        private const string kConfigKey = "Configs/Audio Configuration";

        private readonly List<GameObject> m_goAudious;
        private AudioListener m_audioListener;

        private readonly List<AudioSource> m_musicChannels = new();
        private readonly List<AudioSource> m_soundChannels = new();
        private readonly CancellationTokenSource m_tokenSource;

        private bool m_muteMusic;
        private bool m_muteSounds;
        private float m_volumeMusic;
        private float m_volumeSounds;
        private AudioConfig m_config;

        #endregion

        #region Properties

        private UniTask<AssetsManager> Assets => AssetsManager.Instance;
        private CancellationToken CancellationToken => m_tokenSource.Token;

        #endregion

        #region LifeCycle

        public AudioManager(Transform parent)
        {
            m_goAudious = new List<GameObject>();
            m_tokenSource = new CancellationTokenSource();
            Initialize(parent);
        }

        private void Initialize(Transform parent)
        {
            LoadConfig();

            foreach (var data in m_config.Data)
            {
                var isMusic = data.Type == ChanelType.Music;
                var channels = isMusic ? m_musicChannels : m_soundChannels;

                for (int i = 0; i < data.Capacity; i++)
                {
                    var goAudio = new GameObject(data.Type.ToString() + (i + 1));
                    goAudio.transform.SetParent(parent);
                    m_goAudious.Add(goAudio);

                    var channel = goAudio.AddComponent<AudioSource>();
                    channel.ParseSettings(data.Settings);
                    channels.Add(channel);
                }
            }

            SetupAudioListener(parent);
        }

        public void Dispose()
        {
            m_tokenSource?.Cancel();
            m_musicChannels.Clear();
            m_soundChannels.Clear();
            ClearUpWorld();
        }
        
        private void ClearUpWorld()
        {
            foreach (var goAudio in m_goAudious)
            {
                Object.Destroy(goAudio);
            }

            m_goAudious.Clear();

            Object.Destroy(m_audioListener);
        }

        #endregion

        #region WarmUp

        public async UniTask Preload(params string[] names)
        {
            if (names.IsNullOrEmpty())
            {
                if (kShowLogs)
                {
                    Debug.LogWarning("No keys for preload");
                }
            }

            var tasks = new List<UniTask<LoadingClip>>();

            foreach (var name in names)
            {
                tasks.Add(Load(name));
            }

            await UniTask.WhenAll(tasks);
        }

        public void Preload(params SoundType[] types)
        {
            var names = types.Select(type => type.ToString()).ToArray();
            Preload(names);
        }

        #endregion

        #region Assets

        private void LoadConfig()
        {
            m_config = Resources.Load<AudioConfig>(kConfigKey);
        }

        private async UniTask<LoadingClip> Load(string name)
        {
            var path = name;
            var assets = await Assets;
            var clip = await assets.Load<AudioClip>(path);

            if (clip == null)
            {
                if (kShowLogs)
                {
                    Debug.LogError("No AudioClip by key: " + name);
                }

                return new LoadingClip(null, false);
            }


            return new LoadingClip(clip, true);
        }

        #endregion

        private async UniTask Play(string name, AudioSource channel)
        {
            var response = await Load(name);

            if (!response.Success || channel == null)
            {
                return;
            }

            channel.clip = response.Clip;
            channel.Play();
        }

        private async UniTask PlayWithStop(string name, AudioSource channel)
        {
            await Play(name, channel);

            if (channel.loop)
            {
                return;
            }

            await channel.ToUniTask(CancellationToken);

            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }

            channel.StopSingle();
        }

        private void SetupAudioListener(Transform parent)
        {
            m_audioListener = parent.gameObject.AddComponent<AudioListener>();
        }

        public void SetActiveListener(bool isActive)
        {
            m_audioListener.enabled = isActive;
        }
    }
}