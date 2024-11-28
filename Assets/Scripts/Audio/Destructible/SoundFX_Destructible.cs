using UnityEngine;

public class SoundFX_Destructible : MonoBehaviour
{
    [SerializeField] private SoundData m_takeDamageSound;
    [SerializeField] private SoundData m_dieSound;

    public void PlayTakeDamageSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_takeDamageSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_takeDamageSound.Volume);
    }

    public void PlayDieSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_dieSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_dieSound.Volume);
    }
}