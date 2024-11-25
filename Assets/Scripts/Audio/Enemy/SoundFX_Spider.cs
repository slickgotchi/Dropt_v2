using UnityEngine;

public class SoundFX_Spider : SoundFX_Enemy
{
    [SerializeField] private SoundData m_jumpAttackSound;

    public void PlayJumpAttackSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_jumpAttackSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_jumpAttackSound.Volume);
    }
}
