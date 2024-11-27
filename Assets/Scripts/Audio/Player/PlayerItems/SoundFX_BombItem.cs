using UnityEngine;

public class SoundFX_BombItem : MonoBehaviour
{
    [SerializeField] private SoundData m_timerSound;
    [SerializeField] private SoundData m_explosionSound;

    public void PlayTimerSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_timerSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_timerSound.Volume);
    }

    public void PlayExplosionSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_explosionSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_explosionSound.Volume);
    }
}
