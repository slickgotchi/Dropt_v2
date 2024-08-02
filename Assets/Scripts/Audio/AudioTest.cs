using Audio.Game;
using UnityEngine;

namespace Audio
{
    public sealed class AudioTest : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                var audioManager = GameAudioManager.Instance;
                audioManager.MuteMusic = !audioManager.MuteMusic;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                var audioManager = GameAudioManager.Instance;
                audioManager.MuteSounds = !audioManager.MuteSounds;
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                var audioManager = GameAudioManager.Instance;
                audioManager.PlayerAbility(0, PlayerAbilityEnum.Aura, Vector3.zero);
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                var audioManager = GameAudioManager.Instance;
                audioManager.PlayerAbility(0, PlayerAbilityEnum.Dash, Vector3.zero);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                var audioManager = GameAudioManager.Instance;
                audioManager.PlayMusic(MusicType.RestFloor);
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                var audioManager = GameAudioManager.Instance;
                audioManager.PlayMusic(MusicType.RestFloor, MusicType.UndergroundForest);
            }
        }
    }
}