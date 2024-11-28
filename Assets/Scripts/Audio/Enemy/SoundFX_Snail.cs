using UnityEngine;

public class SoundFX_Snail : SoundFX_Enemy
{
    [SerializeField] private SoundData m_attackSound;

    public void PlayAttackSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_attackSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_attackSound.Volume);
    }
}
