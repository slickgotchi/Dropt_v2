using UnityEngine;

public class SoundFX_FudWisp : SoundFX_Enemy
{
    [SerializeField] private SoundData m_explodeSound;

    public void PlayExplodeSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_explodeSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_explodeSound.Volume);
    }
}
