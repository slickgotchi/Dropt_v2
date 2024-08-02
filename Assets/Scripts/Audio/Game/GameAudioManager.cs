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

        public static void TryToDispose()
        {
            if (null == m_instance)
                return;

            m_instance.Dispose();
            m_instance = null;
        }

        public bool MuteSounds
        {
            get { return _audioManager.MuteSounds; }
            set { _audioManager.MuteSounds = value; }
        }

        public bool MuteMusic
        {
            get { return _audioManager.MuteMusic; }
            set { _audioManager.MuteMusic = value; }
        }


        private AudioManager _audioManager;

        public GameAudioManager()
        {
        }

        private void Awake()
        {
            _audioManager = new AudioManager(transform);
        }

        private void OnApplicationQuit()
        {
            m_instance = null;
        }

        public void Dispose()
        {
            m_instance = null;
            _audioManager.Dispose();
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
            _audioManager.PlayMusic(types);
        }

        public void PlaySoundForMe(SoundType type, Vector3 position)
        {
            _audioManager.PlaySound(type, position);
        }

        public void PlaySoundForMe(string type, Vector3 position)
        {
            _audioManager.PlaySound(type, position);
        }
        
        public void InitPlayer()
        {
            _audioManager.SetActiveListener(false);
        }

        public void DestroyPlayer()
        {
            _audioManager.SetActiveListener(true);
        }

        private void PlaySoundForAll(SoundType type, Vector3 position, ulong id = 0)
        {
            _audioManager.PlaySound(type, position);

            if (null != PLAY_SOUND)
            {
                PLAY_SOUND.Invoke(type.ToString(), position, id);
            }
        }

        private void PlaySoundForAll(string type, Vector3 position)
        {
            _audioManager.PlaySound(type, position);

            if (null != PLAY_SOUND)
            {
                PLAY_SOUND.Invoke(type, position, 0);
            }
        }
    }
}