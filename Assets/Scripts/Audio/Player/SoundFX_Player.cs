using UnityEngine;

public class SoundFX_Player : MonoBehaviour
{
    [SerializeField] private SoundData m_takeDamageSmallSound;
    [SerializeField] private SoundData m_takeDamageBigSound;

    public void PlayTakeDamageSmallSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_takeDamageSmallSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_takeDamageSmallSound.Volume);
    }

    public void PlayTakeDamageBigSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_takeDamageBigSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_takeDamageBigSound.Volume);
    }
}