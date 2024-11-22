using UnityEngine;

public class SoundFX_Gasbag : SoundFX_Enemy
{
    [SerializeField] private SoundData m_attackSound;
    //[SerializeField] private SoundData m_deathSound;
    [SerializeField] private SoundData m_poisionCloudSound;

    public void PlayAttackSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_attackSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_attackSound.Volume);
    }

    //public void PlayDeathSound()
    //{
    //    AudioManager.Instance.PlaySpatialSFX(m_deathSound.AudioClip,
    //                                         transform.position,
    //                                         false,
    //                                         m_deathSound.Volume);
    //}

    public void PlayPoisionCloudSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_poisionCloudSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_poisionCloudSound.Volume);
    }
}