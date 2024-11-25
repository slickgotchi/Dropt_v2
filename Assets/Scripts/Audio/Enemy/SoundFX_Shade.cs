using UnityEngine;

public class SoundFX_Shade : SoundFX_Enemy
{
    [SerializeField] private SoundData m_chargeSound;
    [SerializeField] private SoundData m_attackSound;

    public void PlayChargeSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_chargeSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_chargeSound.Volume);
    }

    public void PlayAttackSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_attackSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_attackSound.Volume);
    }
}
