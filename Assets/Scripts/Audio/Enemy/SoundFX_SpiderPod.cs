using UnityEngine;

public class SoundFX_SpiderPod : SoundFX_Enemy
{
    [SerializeField] private SoundData m_burstSound;

    public void PlayBurstSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_burstSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_burstSound.Volume);
    }
}
