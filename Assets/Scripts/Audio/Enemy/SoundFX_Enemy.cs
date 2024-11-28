using UnityEngine;

public class SoundFX_Enemy : MonoBehaviour
{
    [SerializeField] private SoundData m_spawnSound;
    [SerializeField] private SoundData m_takeDamageSound;
    [SerializeField] private SoundData m_dieSound;

    public void PlaySpawnSound()
    {
        Debug.Log("PlaySpawnSound " + gameObject.name);
        AudioManager.Instance.PlaySpatialSFX(m_spawnSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_spawnSound.Volume);
    }

    public void PlayTakeDamageSound()
    {
        Debug.Log("PlayTakeDamageSound" + gameObject.name);
        AudioManager.Instance.PlaySpatialSFX(m_takeDamageSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_takeDamageSound.Volume);
    }

    public void PlayDieSound()
    {
        Debug.Log("PlayDieSound" + gameObject.name);
        AudioManager.Instance.PlaySpatialSFX(m_dieSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_dieSound.Volume);
    }
}