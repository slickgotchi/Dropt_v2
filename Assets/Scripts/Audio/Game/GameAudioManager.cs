using System;
using UnityEngine;

namespace Audio.Game
{
    public sealed class GameAudioManager : MonoBehaviour, IDisposable
    {
        public Action<string, Vector3, ulong> PLAY_SOUND;

        private static GameAudioManager m_instance;

        public static GameAudioManager Instance
        {
            get
            {
                TryToInitialize();
                return m_instance;
            }
        }

        public static void TryToInitialize()
        {
            if (null != m_instance)
                return;

            GameObject go = new GameObject("GameAudioManager", typeof(GameAudioManager));
            GameObject.DontDestroyOnLoad(go);
            m_instance = go.GetComponent<GameAudioManager>();
        }

        private AudioListener m_audioListener;

        public static void TryToDispose()
        {
            if (null == m_instance)
                return;

            m_instance.Dispose();
            m_instance = null;
        }

        public bool MuteSounds
        {
            get => m_audioManager.MuteSounds;
            set => m_audioManager.MuteSounds = value;
        }

        public bool MuteMusic
        {
            get => m_audioManager.MuteMusic;
            set => m_audioManager.MuteMusic = value;
        }

        public float VolumeMusic
        {
            get => m_audioManager.VolumeMusic;
            set => m_audioManager.VolumeMusic = value;
        }
        
        public float VolumeSounds
        {
            get => m_audioManager.VolumeSounds;
            set => m_audioManager.VolumeSounds = value;
        }


        private AudioManager m_audioManager;
        
        private void Awake()
        {
            m_audioManager = new AudioManager(transform);
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (m_audioListener != null) m_audioListener.enabled = false;
            else m_audioListener = GetComponent<AudioListener>();
        }

        private void OnApplicationQuit()
        {
            m_instance = null;
        }

        public void Dispose()
        {
            m_instance = null;
            m_audioManager.Dispose();
            GameObject.Destroy(gameObject);
        }
#region Game triggers
        public void PlayHit(Destructible.Type type, Vector3 position)
        {
            PlaySoundForAll(type.ToString(), position);
        }

        public void EnemyHurt(Vector3 position)
        {
            PlaySoundForAll(SoundType.Enemy_Hurt_1, position);
        }

        public void FallNewLevel(Vector3 position)
        {
            PlaySoundForMe(SoundType.Fall_NewLevel_1, position);
        }

        public void PlayerAbility(ulong id, PlayerAbilityEnum type, Vector3 position)
        {
            var sound = (type == PlayerAbilityEnum.Dash) ? SoundType.Dash : SoundType.AttackSwipe;
            PlaySoundForAll(sound, position, id);
        }
        #endregion

        public void PlayMusic(params MusicType[] types)
        {
            m_audioManager.PlayMusic(types);
        }

        public void PlaySoundForMe(SoundType type, Vector3 position)
        {
            m_audioManager.PlaySound(type, position);
        }

        public void PlaySoundForMe(string type, Vector3 position)
        {
            m_audioManager.PlaySound(type, position);
        }
        
        public void InitPlayer()
        {
            m_audioManager.SetActiveListener(false);
        }

        public void DestroyPlayer()
        {
            m_audioManager.SetActiveListener(true);
        }

        private void PlaySoundForAll(SoundType type, Vector3 position, ulong id = 0)
        {
            m_audioManager.PlaySound(type, position);

            if (null != PLAY_SOUND)
            {
                PLAY_SOUND.Invoke(type.ToString(), position, id);
            }
        }

        private void PlaySoundForAll(string type, Vector3 position)
        {
            m_audioManager.PlaySound(type, position);

            if (null != PLAY_SOUND)
            {
                PLAY_SOUND.Invoke(type, position, 0);
            }
        }
    }
}