using UnityEngine;

public class SoundFX_FudSpirit : SoundFX_Enemy
{
    [SerializeField] private SoundData m_fadeinSound;
    [SerializeField] private SoundData m_fadeoutSound;
    [SerializeField] private SoundData m_shootSound;

    public void PlayFadeInSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_fadeinSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_fadeinSound.Volume);
    }

    public void PlayFadeOutSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_fadeoutSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_fadeoutSound.Volume);
    }

    public void PlayShootSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_shootSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_shootSound.Volume);
    }
}