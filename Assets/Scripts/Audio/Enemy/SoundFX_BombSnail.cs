using UnityEngine;

public class SoundFX_BombSnail : SoundFX_Enemy
{
    [SerializeField] private SoundData m_igniteSound;
    [SerializeField] private SoundData m_explodeSound;

    public void PlayIgniteSound()
    {
        //Debug.Log("PlayIgniteSound" + gameObject.name);
        AudioManager.Instance.PlaySpatialSFX(m_igniteSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_igniteSound.Volume);
    }

    public void PlayExplodeSound()
    {
        //Debug.Log("PlayExplodeSound" + gameObject.name);
        AudioManager.Instance.PlaySpatialSFX(m_explodeSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_explodeSound.Volume);
    }
}
