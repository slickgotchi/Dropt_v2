using UnityEngine;

public class SoundFX_WispHollow : SoundFX_Enemy
{
    [SerializeField] private SoundData m_spawnWispSound;

    public void PlaySpawnWispSound()
    {
        AudioManager.Instance.PlaySpatialSFX(m_spawnWispSound.AudioClip,
                                             transform.position,
                                             false,
                                             m_spawnWispSound.Volume);
    }
}